using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using INMS.Application.Interfaces;
using INMS.Domain.Enums;
using INMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace INMS.Application.Services
{
    public class ChatService : IChatService
    {
        private const string OllamaUrl = "http://localhost:11434/api/generate";
        private const string OllamaModel = "llama3";
        private const int OllamaRequestTimeoutSeconds = 10;
        private const int OllamaMaxResponseTokens = 120;
        private const string GeneralSystemPrompt = @"You are a Network Operations Assistant for an Integrated Network Management System.
You understand SLBN, CEAN, and MSAN layers.
Answer only network-related questions.
If the question needs live data, guide the user toward supported queries such as total nodes, active nodes, down nodes, active alarms, device status, critical alarms, root cause, or impacted devices.
Keep responses under four short sentences.
Be concise and technical.";
        private const int AlarmPreviewLimit = 10;
        private const int ImpactedDevicePreviewLimit = 10;
        private const int DevicePreviewLimit = 10;

        private static readonly string[] DeviceStatusMarkers =
        [
            "device status of",
            "device status for",
            "status of",
            "status for"
        ];

        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ChatService> _logger;
        private static readonly SemaphoreSlim _deviceNamesCacheLock = new SemaphoreSlim(1, 1);
        private static List<string>? _cachedDeviceNames = null;
        private static DateTime _cacheExpiry = DateTime.MinValue;

        public ChatService(HttpClient httpClient, AppDbContext dbContext, ILogger<ChatService> logger)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(OllamaRequestTimeoutSeconds);
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Please enter a message so I can help.";
            }

            var trimmedMessage = userMessage.Trim();
            var fastGeneralResponse = TryBuildFastGeneralResponse(trimmedMessage);
            if (fastGeneralResponse != null)
            {
                _logger.LogInformation("Chat request completed with fast general response.");
                return fastGeneralResponse;
            }

            var intent = DetectIntent(trimmedMessage);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = intent switch
                {
                    ChatIntent.TotalNodes => await HandleTotalNodesIntentAsync(trimmedMessage),
                    ChatIntent.ActiveNodes => await HandleActiveNodesIntentAsync(trimmedMessage),
                    ChatIntent.DownNodes => await HandleDownNodesIntentAsync(trimmedMessage),
                    ChatIntent.ActiveAlarms => await HandleActiveAlarmsIntentAsync(trimmedMessage),
                    ChatIntent.NetworkOverview => await HandleNetworkOverviewIntentAsync(trimmedMessage),
                    ChatIntent.DeviceStatus => await HandleDeviceStatusIntentAsync(trimmedMessage),
                    ChatIntent.CriticalAlarms => await HandleCriticalAlarmsIntentAsync(trimmedMessage),
                    ChatIntent.RootCause => await HandleRootCauseIntentAsync(trimmedMessage),
                    ChatIntent.ImpactedDevices => await HandleImpactedDevicesIntentAsync(trimmedMessage),
                    _ => await GenerateGeneralResponseAsync(trimmedMessage)
                };

                _logger.LogInformation(
                    "Chat request for intent {Intent} completed in {ElapsedMilliseconds} ms.",
                    intent,
                    stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "Database update error while processing chat request after {ElapsedMilliseconds} ms: {UserMessage}",
                    stopwatch.ElapsedMilliseconds,
                    trimmedMessage);
                _cachedDeviceNames = null;  // Invalidate cache on DB update error
                return "I couldn't read the latest network data because the database operation failed. Please try again.";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(
                    ex,
                    "Database read error while processing chat request after {ElapsedMilliseconds} ms: {UserMessage}",
                    stopwatch.ElapsedMilliseconds,
                    trimmedMessage);
                return "I couldn't process the live database results for that request. Please try again.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Ollama request failed while processing chat request after {ElapsedMilliseconds} ms: {UserMessage}",
                    stopwatch.ElapsedMilliseconds,
                    trimmedMessage);
                return "I couldn't reach the local Ollama service. Please make sure Ollama is running and the llama3 model is available.";
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "Ollama request timed out while processing chat request after {ElapsedMilliseconds} ms: {UserMessage}",
                    stopwatch.ElapsedMilliseconds,
                    trimmedMessage);
                return "The AI response timed out. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while processing chat request after {ElapsedMilliseconds} ms: {UserMessage}",
                    stopwatch.ElapsedMilliseconds,
                    trimmedMessage);
                return "I encountered an unexpected error while processing that request. Please try again.";
            }
        }

        public async Task<IReadOnlyList<AlarmSummary>> GetActiveAlarmsAsync()
        {
            var rows = await (
                from alarm in _dbContext.Alarms.AsNoTracking()
                join device in _dbContext.Devices.AsNoTracking() on alarm.DeviceId equals device.DeviceId into deviceGroup
                from device in deviceGroup.DefaultIfEmpty()
                where alarm.IsActive
                orderby alarm.RaisedTime descending
                select new
                {
                    alarm.AlarmId,
                    alarm.DeviceId,
                    DeviceName = device == null ? null : device.DeviceName,
                    alarm.AlarmType,
                    alarm.RaisedTime,
                    Status = device == null ? (DeviceStatus?)null : device.Status,
                    PriorityLevel = device == null ? (PriorityLevel?)null : device.PriorityLevel
                })
                .ToListAsync();

            return rows
                .Select(row => new AlarmSummary(
                    row.AlarmId,
                    row.DeviceId,
                    row.DeviceName ?? $"Device #{row.DeviceId}",
                    row.AlarmType,
                    row.RaisedTime,
                    row.Status ?? DeviceStatus.UNREACHABLE,
                    row.PriorityLevel ?? PriorityLevel.Low,
                    row.DeviceName != null))
                .ToList();
        }

        public async Task<ActiveNodeSummary> GetActiveNodeCountAsync()
        {
            var summary = await _dbContext.Devices
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    TotalNodes = group.Count(),
                    ActiveNodes = group.Sum(device => device.Status == DeviceStatus.UP ? 1 : 0),
                    DownNodes = group.Sum(device =>
                        device.Status == DeviceStatus.DOWN || device.Status == DeviceStatus.UNREACHABLE ? 1 : 0)
                })
                .FirstOrDefaultAsync();

            return summary == null
                ? new ActiveNodeSummary(0, 0, 0)
                : new ActiveNodeSummary(summary.TotalNodes, summary.ActiveNodes, summary.DownNodes);
        }

        public async Task<DeviceStatusSummary?> GetDeviceStatusAsync(string deviceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                return null;
            }

            var cleanedDeviceName = CleanupExtractedValue(deviceName);
            var resolvedDeviceName = await ResolveDeviceNameAsync(cleanedDeviceName) ?? cleanedDeviceName;

            var matchedDevice = await _dbContext.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(device => device.DeviceName == resolvedDeviceName);

            if (matchedDevice == null)
            {
                return null;
            }

            var alarmSummary = await _dbContext.Alarms
                .AsNoTracking()
                .Where(alarm => alarm.DeviceId == matchedDevice.DeviceId)
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    ActiveAlarmCount = group.Sum(alarm => alarm.IsActive ? 1 : 0),
                    LatestAlarmRaisedTime = group.Max(alarm => (DateTime?)alarm.RaisedTime)
                })
                .FirstOrDefaultAsync();

            return new DeviceStatusSummary(
                matchedDevice.DeviceId,
                matchedDevice.DeviceName,
                matchedDevice.DeviceType.ToString(),
                matchedDevice.Status,
                matchedDevice.PriorityLevel,
                matchedDevice.IP,
                matchedDevice.LEAId,
                matchedDevice.IsSimulatedDown,
                alarmSummary?.ActiveAlarmCount ?? 0,
                alarmSummary?.LatestAlarmRaisedTime);
        }

        public async Task<IReadOnlyList<AlarmSummary>> GetCriticalAlarmsAsync()
        {
            var rows = await (
                from alarm in _dbContext.Alarms.AsNoTracking()
                join device in _dbContext.Devices.AsNoTracking() on alarm.DeviceId equals device.DeviceId
                where alarm.IsActive && device.PriorityLevel == PriorityLevel.Critical
                orderby alarm.RaisedTime descending
                select new
                {
                    alarm.AlarmId,
                    alarm.DeviceId,
                    device.DeviceName,
                    alarm.AlarmType,
                    alarm.RaisedTime,
                    device.Status,
                    device.PriorityLevel
                })
                .ToListAsync();

            return rows
                .Select(row => new AlarmSummary(
                    row.AlarmId,
                    row.DeviceId,
                    row.DeviceName,
                    row.AlarmType,
                    row.RaisedTime,
                    row.Status,
                    row.PriorityLevel))
                .ToList();
        }

        public async Task<RootCauseSummary?> GetRootCauseAsync(int alarmId)
        {
            var row = await (
                from rootCause in _dbContext.RootCauses.AsNoTracking()
                join device in _dbContext.Devices.AsNoTracking() on rootCause.RootCauseDeviceId equals device.DeviceId
                where rootCause.AlarmId == alarmId
                orderby rootCause.DetectedTime descending
                select new
                {
                    rootCause.RootCauseId,
                    rootCause.AlarmId,
                    rootCause.RootCauseDeviceId,
                    device.DeviceName,
                    rootCause.RootCauseType,
                    rootCause.DetectedTime
                })
                .FirstOrDefaultAsync();

            if (row == null)
            {
                return null;
            }

            return new RootCauseSummary(
                row.RootCauseId,
                row.AlarmId,
                row.RootCauseDeviceId,
                row.DeviceName,
                row.RootCauseType,
                row.DetectedTime);
        }

        public async Task<IReadOnlyList<ImpactedDeviceSummary>> GetImpactedDevicesAsync(int alarmId)
        {
            var latestRootCause = await _dbContext.RootCauses
                .AsNoTracking()
                .Where(rootCause => rootCause.AlarmId == alarmId)
                .OrderByDescending(rootCause => rootCause.DetectedTime)
                .Select(rootCause => new { rootCause.RootCauseId })
                .FirstOrDefaultAsync();

            if (latestRootCause == null)
            {
                return [];
            }

            var rows = await (
                from impactedDevice in _dbContext.ImpactedDevices.AsNoTracking()
                join device in _dbContext.Devices.AsNoTracking() on impactedDevice.DeviceId equals device.DeviceId
                where impactedDevice.RootCauseId == latestRootCause.RootCauseId
                orderby device.DeviceName
                select new
                {
                    impactedDevice.ImpactId,
                    impactedDevice.RootCauseId,
                    device.DeviceId,
                    device.DeviceName,
                    device.DeviceType,
                    device.Status,
                    impactedDevice.ImpactType
                })
                .ToListAsync();

            return rows
                .Select(row => new ImpactedDeviceSummary(
                    row.ImpactId,
                    row.RootCauseId,
                    row.DeviceId,
                    row.DeviceName,
                    row.DeviceType.ToString(),
                    row.Status,
                    row.ImpactType))
                .ToList();
        }

        private async Task<string> HandleTotalNodesIntentAsync(string userMessage)
        {
            var nodeSummary = await GetActiveNodeCountAsync();
            return $"There are {nodeSummary.TotalNodes} total nodes in the network: {nodeSummary.ActiveNodes} active and {nodeSummary.DownNodes} down or unreachable.";
        }

        private async Task<string> HandleActiveNodesIntentAsync(string userMessage)
        {
            if (WantsList(userMessage))
            {
                var snapshot = await GetDeviceSnapshotAsync(activeOnly: true, DevicePreviewLimit);
                return BuildDeviceSnapshotResponse(snapshot, "active nodes");
            }

            var activeNodeSummary = await GetActiveNodeCountAsync();
            return $"{activeNodeSummary.ActiveNodes} of {activeNodeSummary.TotalNodes} network nodes are currently active. {activeNodeSummary.DownNodes} are down or unreachable.";
        }

        private async Task<string> HandleDownNodesIntentAsync(string userMessage)
        {
            if (WantsList(userMessage))
            {
                var snapshot = await GetDeviceSnapshotAsync(activeOnly: false, DevicePreviewLimit);
                return BuildDeviceSnapshotResponse(snapshot, "down or unreachable nodes");
            }

            var nodeSummary = await GetActiveNodeCountAsync();
            return $"{nodeSummary.DownNodes} network nodes are currently down or unreachable. {nodeSummary.ActiveNodes} remain active out of {nodeSummary.TotalNodes} total nodes.";
        }

        private async Task<string> HandleActiveAlarmsIntentAsync(string userMessage)
        {
            var snapshot = await GetAlarmSnapshotAsync(criticalOnly: false, AlarmPreviewLimit);
            return BuildAlarmSnapshotResponse(snapshot, "active alarms");
        }

        private async Task<string> HandleNetworkOverviewIntentAsync(string userMessage)
        {
            var nodeSummary = await GetActiveNodeCountAsync();
            var activeAlarmCount = await _dbContext.Alarms
                .AsNoTracking()
                .CountAsync(alarm => alarm.IsActive);

            var criticalAlarmCount = await (
                from alarm in _dbContext.Alarms.AsNoTracking()
                join device in _dbContext.Devices.AsNoTracking() on alarm.DeviceId equals device.DeviceId
                where alarm.IsActive && device.PriorityLevel == PriorityLevel.Critical
                select alarm.AlarmId)
                .CountAsync();

            return $"Network overview: {nodeSummary.ActiveNodes}/{nodeSummary.TotalNodes} nodes are active, {nodeSummary.DownNodes} are down or unreachable, and there are {activeAlarmCount} active alarms. Critical-priority active alarms: {criticalAlarmCount}.";
        }

        private async Task<string> HandleDeviceStatusIntentAsync(string userMessage)
        {
            var deviceName = await ExtractDeviceNameAsync(userMessage);
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                return "Please include the device name when asking for device status. Example: device status of SLBN-Colombo-01.";
            }

            var device = await GetDeviceStatusAsync(deviceName);
            if (device == null)
            {
                return $"I couldn't find a device named '{deviceName}'. Please provide a valid device name.";
            }

            return BuildDeviceStatusResponse(device);
        }

        private async Task<string> HandleCriticalAlarmsIntentAsync(string userMessage)
        {
            var snapshot = await GetAlarmSnapshotAsync(criticalOnly: true, AlarmPreviewLimit);
            return BuildAlarmSnapshotResponse(snapshot, "critical alarms");
        }

        private async Task<string> HandleRootCauseIntentAsync(string userMessage)
        {
            var alarmId = ExtractAlarmId(userMessage);
            if (alarmId == null)
            {
                return "Please include an alarm ID when asking for a root cause. Example: root cause for alarm 12.";
            }

            var rootCause = await GetRootCauseAsync(alarmId.Value);
            if (rootCause == null)
            {
                return $"No root cause record was found for alarm ID {alarmId.Value}.";
            }

            return BuildRootCauseResponse(rootCause);
        }

        private async Task<string> HandleImpactedDevicesIntentAsync(string userMessage)
        {
            var alarmId = ExtractAlarmId(userMessage);
            if (alarmId == null)
            {
                return "Please include an alarm ID when asking for impacted devices. Example: impacted devices for alarm 12.";
            }

            var impactedDevices = await GetImpactedDevicesAsync(alarmId.Value);
            if (impactedDevices.Count == 0)
            {
                return $"No impacted devices were found for alarm ID {alarmId.Value}.";
            }

            return BuildImpactedDevicesResponse(alarmId.Value, impactedDevices);
        }

        private async Task<string> GenerateGeneralResponseAsync(string userMessage)
        {
            var prompt = $"{GeneralSystemPrompt}\n\nUser: {userMessage}\nAssistant:";
            return await RequestOllamaResponseAsync(prompt);
        }

        private async Task<AlarmSnapshot> GetAlarmSnapshotAsync(bool criticalOnly, int limit)
        {
            var query =
                from alarm in _dbContext.Alarms.AsNoTracking()
                join device in _dbContext.Devices.AsNoTracking() on alarm.DeviceId equals device.DeviceId into deviceGroup
                from device in deviceGroup.DefaultIfEmpty()
                where alarm.IsActive && (!criticalOnly || (device != null && device.PriorityLevel == PriorityLevel.Critical))
                select new
                {
                    alarm.AlarmId,
                    alarm.DeviceId,
                    DeviceName = device == null ? null : device.DeviceName,
                    alarm.AlarmType,
                    alarm.RaisedTime,
                    Status = device == null ? (DeviceStatus?)null : device.Status,
                    PriorityLevel = device == null ? (PriorityLevel?)null : device.PriorityLevel
                };

            var totalCount = await query.CountAsync();
            var rows = await query
                .OrderByDescending(row => row.RaisedTime)
                .Take(limit)
                .ToListAsync();

            return new AlarmSnapshot(
                totalCount,
                rows
                    .Select(row => new AlarmSummary(
                        row.AlarmId,
                        row.DeviceId,
                        row.DeviceName ?? $"Device #{row.DeviceId}",
                        row.AlarmType,
                        row.RaisedTime,
                        row.Status ?? DeviceStatus.UNREACHABLE,
                        row.PriorityLevel ?? PriorityLevel.Low,
                        row.DeviceName != null))
                    .ToList());
        }

        private async Task<DeviceSnapshot> GetDeviceSnapshotAsync(bool activeOnly, int limit)
        {
            var query = _dbContext.Devices.AsNoTracking();

            query = activeOnly
                ? query.Where(device => device.Status == DeviceStatus.UP)
                : query.Where(device =>
                    device.Status == DeviceStatus.DOWN ||
                    device.Status == DeviceStatus.UNREACHABLE ||
                    device.IsSimulatedDown);

            var totalCount = await query.CountAsync();
            var rows = await query
                .OrderByDescending(device => device.PriorityLevel)
                .ThenBy(device => device.DeviceName)
                .Take(limit)
                .Select(device => new DeviceSummary(
                    device.DeviceId,
                    device.DeviceName,
                    device.DeviceType.ToString(),
                    device.Status,
                    device.PriorityLevel,
                    device.IP))
                .ToListAsync();

            return new DeviceSnapshot(totalCount, rows);
        }

        private async Task<string> RequestOllamaResponseAsync(string prompt)
        {
            var requestBody = new
            {
                model = OllamaModel,
                prompt,
                stream = false,
                keep_alive = "15m",
                options = new
                {
                    temperature = 0.2,
                    top_p = 0.9,
                    num_predict = OllamaMaxResponseTokens
                }
            };

            var response = await _httpClient.PostAsJsonAsync(OllamaUrl, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get response from Ollama: {response.StatusCode} - {errorBody}");
            }

            var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaResponse>();
            return ollamaResponse?.Response?.Trim() ?? "I couldn't generate a response from the AI model.";
        }

        private static ChatIntent DetectIntent(string userMessage)
        {
            var normalized = userMessage.ToLowerInvariant();

            if (ContainsAny(normalized,
                    "total nodes",
                    "total node count",
                    "how many nodes are there",
                    "how many total nodes",
                    "how many devices are there",
                    "how many total devices",
                    "number of nodes",
                    "number of devices",
                    "node count",
                    "device count",
                    "node count eka",
                    "device count eka",
                    "nodes keeyada",
                    "devices keeyada",
                    "node ganana",
                    "device ganana",
                    "count of nodes",
                    "count of devices",
                    "all nodes count"))
            {
                return ChatIntent.TotalNodes;
            }

            if (ContainsAny(normalized,
                    "active nodes",
                    "active devices",
                    "reachable nodes",
                    "reachable devices",
                    "active node count",
                    "up nodes",
                    "up devices",
                    "online nodes",
                    "online devices",
                    "healthy nodes",
                    "working nodes",
                    "running nodes",
                    "available nodes",
                    "how many are up",
                    "how many up",
                    "active nodes keeyada",
                    "active devices keeyada",
                    "up nodes keeyada",
                    "online nodes keeyada"))
            {
                return ChatIntent.ActiveNodes;
            }

            if (ContainsAny(normalized,
                    "down nodes",
                    "down devices",
                    "failed nodes",
                    "failed devices",
                    "unreachable nodes",
                    "unreachable devices",
                    "offline nodes",
                    "offline devices",
                    "faulty nodes",
                    "faulty devices",
                    "not working nodes",
                    "not working devices",
                    "how many are down",
                    "how many down",
                    "down nodes keeyada",
                    "down devices keeyada",
                    "offline nodes keeyada",
                    "offline devices keeyada",
                    "down nodes tika",
                    "down devices tika"))
            {
                return ChatIntent.DownNodes;
            }

            if (ContainsAny(normalized,
                    "impacted devices",
                    "affected devices",
                    "devices impacted"))
            {
                return ChatIntent.ImpactedDevices;
            }

            if (ContainsAny(normalized,
                    "root cause",
                    "cause of alarm",
                    "why did this alarm happen"))
            {
                return ChatIntent.RootCause;
            }

            if (ContainsAny(normalized,
                    "network overview",
                    "network summary",
                    "network health",
                    "system overview",
                    "system summary",
                    "system status",
                    "overall status",
                    "overall health",
                    "current situation",
                    "dashboard summary",
                    "system insights",
                    "network state",
                    "network summary eka",
                    "network health eka",
                    "system status eka"))
            {
                return ChatIntent.NetworkOverview;
            }

            if (ContainsAny(normalized,
                    "device status",
                    "status of",
                    "status for",
                    "device health",
                    "device state") ||
                Regex.IsMatch(normalized, @"\bis\s+.+\b(up|down|offline|online|reachable|healthy)\b") ||
                Regex.IsMatch(normalized, @"\b(?:check|show|tell me|what is)\s+.+\bstatus\b"))
            {
                return ChatIntent.DeviceStatus;
            }

            if (ContainsAny(normalized,
                    "active alarms",
                    "active alarm",
                    "current alarms",
                    "current alarm",
                    "open alarms",
                    "open alarm",
                    "show alarms",
                    "list alarms",
                    "alarm list",
                    "alarms list",
                    "fault list",
                    "current faults",
                    "open faults",
                    "alarms pennanna",
                    "alarms penwanna",
                    "alarms balanna",
                    "active alarms tika",
                    "current alarms tika",
                    "how many alarms are active",
                    "how many alarms"))
            {
                return ChatIntent.ActiveAlarms;
            }

            if (ContainsAny(normalized,
                    "critical alarms",
                    "critical alarm",
                    "critical severity",
                    "highest priority alarms") ||
                (normalized.Contains("critical") && normalized.Contains("alarm")))
            {
                return ChatIntent.CriticalAlarms;
            }

            return ChatIntent.General;
        }

        private static string? TryBuildFastGeneralResponse(string userMessage)
        {
            var normalized = userMessage.ToLowerInvariant();
            var hasAlarmId = ExtractAlarmId(userMessage).HasValue;

            if (IsGreeting(normalized))
            {
                return "Hi. I can help with INMS network status, active/down nodes, alarms, device health, root cause, and impacted devices. Try asking: network summary, show active alarms, or status of SLBN-Colombo-01.";
            }

            if (ContainsAny(normalized,
                    "help",
                    "what can you do",
                    "how can you help",
                    "commands",
                    "sample questions",
                    "example questions",
                    "what should i ask",
                    "questions can i ask"))
            {
                return "You can ask me for: network summary, total nodes, active nodes, down nodes, active alarms, critical alarms, device status, root cause for an alarm ID, or impacted devices for an alarm ID.";
            }

            if (ContainsAny(normalized,
                    "what is slbn",
                    "what's slbn",
                    "define slbn",
                    "slbn means",
                    "about slbn",
                    "slbn layer",
                    "slbn mokakda",
                    "slbn mokadda",
                    "slbn kiyanne"))
            {
                return "SLBN is the service or backbone layer represented in this INMS topology. It is treated as an upstream network layer, so a fault there can affect lower-layer CEAN/MSAN devices and services.";
            }

            if (ContainsAny(normalized,
                    "what is cean",
                    "what's cean",
                    "define cean",
                    "cean means",
                    "about cean",
                    "cean layer",
                    "cean mokakda",
                    "cean mokadda",
                    "cean kiyanne"))
            {
                return "CEAN is an aggregation/access network layer in this INMS model. CEAN devices sit between upstream SLBN nodes and downstream access nodes, so their failures can create wider service impact.";
            }

            if (ContainsAny(normalized,
                    "what is msan",
                    "what's msan",
                    "define msan",
                    "msan means",
                    "about msan",
                    "msan layer",
                    "msan mokakda",
                    "msan mokadda",
                    "msan kiyanne"))
            {
                return "MSAN stands for Multi-Service Access Node. In this system it represents an access-layer device that can serve downstream customers or services and can be impacted by upstream faults.";
            }

            if (ContainsAny(normalized,
                    "what is inms",
                    "what's inms",
                    "define inms",
                    "about inms",
                    "alarm management system",
                    "integrated network management system",
                    "inms mokakda",
                    "inms mokadda",
                    "inms kiyanne"))
            {
                return "INMS is the Integrated Network Management System used here to monitor devices, raise alarms, simulate failures, identify root causes, and show impacted devices across the telecom topology.";
            }

            if (!hasAlarmId && ContainsAny(normalized,
                    "what is root cause",
                    "what's root cause",
                    "define root cause",
                    "explain root cause",
                    "root cause analysis",
                    "root cause mokakda",
                    "root cause mokadda",
                    "root cause kiyanne"))
            {
                return "Root cause is the upstream or primary device/fault that triggered one or more downstream alarms. Ask 'root cause for alarm 12' when you want the system to look up a specific alarm.";
            }

            if (!hasAlarmId && ContainsAny(normalized,
                    "what are impacted devices",
                    "what is impacted device",
                    "define impacted devices",
                    "explain impacted devices",
                    "impact analysis",
                    "impacted devices monawada",
                    "impacted devices mokakda",
                    "impacted devices kiyanne"))
            {
                return "Impacted devices are downstream devices affected by a detected root cause. Ask 'impacted devices for alarm 12' to list the affected devices for a specific alarm.";
            }

            if (ContainsAny(normalized,
                    "alarm correlation",
                    "what is correlation",
                    "explain correlation",
                    "correlate alarms"))
            {
                return "Alarm correlation groups related alarms so the operator can focus on the likely root cause instead of treating every downstream alarm as a separate failure.";
            }

            if (ContainsAny(normalized,
                    "what is alarm",
                    "what are alarms",
                    "define alarm",
                    "explain alarm",
                    "alarm mokakda",
                    "alarm mokadda",
                    "alarm kiyanne",
                    "alarms monawada"))
            {
                return "An alarm is a fault or warning raised against a network device. In this system active alarms show current unresolved issues, while critical alarms highlight faults on critical-priority devices.";
            }

            if (ContainsAny(normalized,
                    "what is device status",
                    "define device status",
                    "explain device status",
                    "device status mokakda",
                    "device status kiyanne",
                    "status meanings"))
            {
                return "Device status shows the current operational state of a node: UP means healthy, DOWN means failed, UNREACHABLE means it cannot be reached, and IMPACTED means it is affected by another fault.";
            }

            if (!ContainsAny(normalized, "alarm", "alarms", "fault", "faults") &&
                ContainsAny(normalized,
                    "priority level",
                    "critical priority",
                    "what is critical",
                    "severity",
                    "priority mokakda",
                    "priority kiyanne",
                    "severity kiyanne"))
            {
                return "Priority indicates the operational importance of a device or alarm context. Critical-priority devices are the highest concern because a failure there can create wider service impact.";
            }

            if (ContainsAny(normalized,
                    "network topology",
                    "topology",
                    "topology eka",
                    "network hierarchy",
                    "hierarchy"))
            {
                return "The INMS topology models telecom devices in upstream and downstream layers such as SLBN, CEAN, and MSAN. This hierarchy helps the system identify which downstream devices may be impacted when an upstream node fails.";
            }

            if (ContainsAny(normalized,
                    "failure simulation",
                    "simulate failure",
                    "simulate node",
                    "simulation eka",
                    "device down simulation"))
            {
                return "Failure simulation marks a device as down so INMS can raise alarms, detect likely root cause, and calculate downstream impact. Use the device simulation actions in the system when you want to test alarm behavior.";
            }

            if (!IsNetworkRelated(normalized))
            {
                return "I am focused on INMS and network operations. Ask me about nodes, alarms, device status, root cause, impacted devices, SLBN, CEAN, or MSAN.";
            }

            return null;
        }

        private static bool IsGreeting(string normalized)
        {
            var cleaned = normalized.Trim().Trim('.', '?', '!', ',', ':', ';');
            return cleaned is "hi" or "hello" or "hey" or "hai" or "hii" or "good morning" or "good afternoon" or "good evening";
        }

        private static bool IsNetworkRelated(string normalized)
        {
            return ContainsAny(normalized,
                "network",
                "node",
                "nodes",
                "device",
                "devices",
                "alarm",
                "alarms",
                "fault",
                "faults",
                "failure",
                "failures",
                "outage",
                "status",
                "health",
                "root cause",
                "impact",
                "impacted",
                "slbn",
                "cean",
                "msan",
                "inms",
                "nms",
                "topology",
                "simulation",
                "simulate",
                "critical",
                "priority");
        }

        private static bool WantsList(string userMessage)
        {
            return ContainsAny(userMessage,
                "show",
                "list",
                "which",
                "what are",
                "names",
                "details",
                "display",
                "tika",
                "pennanna",
                "penwanna",
                "balanna");
        }

        private static bool ContainsAny(string value, params string[] phrases)
        {
            return phrases.Any(phrase => value.Contains(phrase, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<string?> ExtractDeviceNameAsync(string userMessage)
        {
            var quotedMatch = Regex.Match(userMessage, "[\"'](?<value>[^\"']+)[\"']", RegexOptions.IgnoreCase);
            if (quotedMatch.Success)
            {
                return await ResolveDeviceNameAsync(quotedMatch.Groups["value"].Value.Trim())
                    ?? quotedMatch.Groups["value"].Value.Trim();
            }

            foreach (var marker in DeviceStatusMarkers)
            {
                var markerIndex = userMessage.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (markerIndex < 0)
                {
                    continue;
                }

                var startIndex = markerIndex + marker.Length;
                var candidate = CleanupExtractedValue(userMessage[startIndex..]);
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return await ResolveDeviceNameAsync(candidate) ?? candidate;
                }
            }

            // Try to match against cached device names first
            return await ResolveDeviceNameAsync(userMessage);
        }

        private async Task<List<string>> GetCachedDeviceNamesAsync()
        {
            // Return cached names if cache is still valid (2 minutes)
            if (_cachedDeviceNames != null && DateTime.UtcNow < _cacheExpiry)
            {
                return _cachedDeviceNames;
            }

            await _deviceNamesCacheLock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (_cachedDeviceNames != null && DateTime.UtcNow < _cacheExpiry)
                {
                    return _cachedDeviceNames;
                }

                _cachedDeviceNames = await _dbContext.Devices
                    .AsNoTracking()
                    .Select(device => device.DeviceName)
                    .ToListAsync();
                
                _cacheExpiry = DateTime.UtcNow.AddMinutes(2);
                return _cachedDeviceNames;
            }
            finally
            {
                _deviceNamesCacheLock.Release();
            }
        }

        private static int? ExtractAlarmId(string userMessage)
        {
            var explicitAlarmMatch = Regex.Match(
                userMessage,
                @"\balarm(?:\s+id)?\s*[:#]?\s*(\d+)\b",
                RegexOptions.IgnoreCase);

            if (explicitAlarmMatch.Success &&
                int.TryParse(explicitAlarmMatch.Groups[1].Value, out var explicitAlarmId))
            {
                return explicitAlarmId;
            }

            var fallbackMatch = Regex.Match(userMessage, @"\b(\d+)\b");
            if (fallbackMatch.Success &&
                int.TryParse(fallbackMatch.Groups[1].Value, out var fallbackAlarmId))
            {
                return fallbackAlarmId;
            }

            return null;
        }

        private static string BuildActiveAlarmsContext(IReadOnlyList<AlarmSummary> alarms)
        {
            if (alarms.Count == 0)
            {
                return "No active alarms were found.";
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Active alarm count: {alarms.Count}");

            foreach (var alarm in alarms)
            {
                var deviceDetails = alarm.DeviceFound
                    ? $"Device={alarm.DeviceName}; DeviceId={alarm.DeviceId}; DeviceStatus={alarm.DeviceStatus}; Priority={alarm.PriorityLevel}"
                    : $"DeviceId={alarm.DeviceId}; Device details unavailable";

                builder.AppendLine(
                    $"- AlarmId={alarm.AlarmId}; {deviceDetails}; AlarmType={alarm.AlarmType}; RaisedTimeUtc={alarm.RaisedTime:O}");
            }

            return builder.ToString().TrimEnd();
        }

        private static string BuildActiveNodesContext(ActiveNodeSummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Network node summary:");
            builder.AppendLine($"- TotalNodes={summary.TotalNodes}");
            builder.AppendLine($"- ActiveNodes={summary.ActiveNodes}");
            builder.AppendLine($"- DownOrUnreachableNodes={summary.DownNodes}");
            return builder.ToString().TrimEnd();
        }

        private static string BuildTotalNodesContext(ActiveNodeSummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Network total node summary:");
            builder.AppendLine($"- TotalNodes={summary.TotalNodes}");
            builder.AppendLine($"- ActiveNodes={summary.ActiveNodes}");
            builder.AppendLine($"- DownOrUnreachableNodes={summary.DownNodes}");
            return builder.ToString().TrimEnd();
        }

        private static string BuildDownNodesContext(ActiveNodeSummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Network down node summary:");
            builder.AppendLine($"- DownOrUnreachableNodes={summary.DownNodes}");
            builder.AppendLine($"- ActiveNodes={summary.ActiveNodes}");
            builder.AppendLine($"- TotalNodes={summary.TotalNodes}");
            return builder.ToString().TrimEnd();
        }

        private static string BuildDeviceStatusContext(DeviceStatusSummary device)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Device status lookup result:");
            builder.AppendLine($"- DeviceId={device.DeviceId}");
            builder.AppendLine($"- DeviceName={device.DeviceName}");
            builder.AppendLine($"- DeviceType={device.DeviceType}");
            builder.AppendLine($"- Status={device.Status}");
            builder.AppendLine($"- PriorityLevel={device.PriorityLevel}");
            builder.AppendLine($"- IP={device.IP}");
            builder.AppendLine($"- LEAId={device.LEAId}");
            builder.AppendLine($"- IsSimulatedDown={device.IsSimulatedDown}");
            builder.AppendLine($"- ActiveAlarmCount={device.ActiveAlarmCount}");
            builder.AppendLine($"- LatestAlarmRaisedTimeUtc={(device.LatestAlarmRaisedTime.HasValue ? device.LatestAlarmRaisedTime.Value.ToString("O") : "None")}");
            return builder.ToString().TrimEnd();
        }

        private static string BuildCriticalAlarmsContext(IReadOnlyList<AlarmSummary> alarms)
        {
            if (alarms.Count == 0)
            {
                return "No active alarms were found on critical-priority devices.";
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Critical alarm count: {alarms.Count}");

            foreach (var alarm in alarms)
            {
                var deviceDetails = alarm.DeviceFound
                    ? $"Device={alarm.DeviceName}; DeviceId={alarm.DeviceId}; DeviceStatus={alarm.DeviceStatus}; Priority={alarm.PriorityLevel}"
                    : $"DeviceId={alarm.DeviceId}; Device details unavailable";

                builder.AppendLine(
                    $"- AlarmId={alarm.AlarmId}; {deviceDetails}; AlarmType={alarm.AlarmType}; RaisedTimeUtc={alarm.RaisedTime:O}");
            }

            return builder.ToString().TrimEnd();
        }

        private static string BuildRootCauseContext(RootCauseSummary rootCause)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Root cause lookup result:");
            builder.AppendLine($"- RootCauseId={rootCause.RootCauseId}");
            builder.AppendLine($"- AlarmId={rootCause.AlarmId}");
            builder.AppendLine($"- RootCauseDeviceId={rootCause.RootCauseDeviceId}");
            builder.AppendLine($"- RootCauseDeviceName={rootCause.RootCauseDeviceName}");
            builder.AppendLine($"- RootCauseType={rootCause.RootCauseType}");
            builder.AppendLine($"- DetectedTimeUtc={rootCause.DetectedTime:O}");
            return builder.ToString().TrimEnd();
        }

        private static string BuildImpactedDevicesContext(int alarmId, IReadOnlyList<ImpactedDeviceSummary> impactedDevices)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Impacted devices for alarm ID {alarmId}: {impactedDevices.Count}");

            foreach (var impactedDevice in impactedDevices)
            {
                builder.AppendLine(
                    $"- ImpactId={impactedDevice.ImpactId}; RootCauseId={impactedDevice.RootCauseId}; DeviceId={impactedDevice.DeviceId}; DeviceName={impactedDevice.DeviceName}; DeviceType={impactedDevice.DeviceType}; Status={impactedDevice.Status}; ImpactType={impactedDevice.ImpactType}");
            }

            return builder.ToString().TrimEnd();
        }

        private static string CleanupExtractedValue(string rawValue)
        {
            return rawValue
                .Trim()
                .Trim('.', '?', '!', ',', ':', ';')
                .Trim();
        }

        private async Task<string?> ResolveDeviceNameAsync(string rawValue)
        {
            var cleanedValue = CleanupExtractedValue(rawValue);
            if (string.IsNullOrWhiteSpace(cleanedValue))
            {
                return null;
            }

            var deviceNames = await GetCachedDeviceNamesAsync();

            var exactMatch = deviceNames.FirstOrDefault(
                deviceName => string.Equals(deviceName, cleanedValue, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null)
            {
                return exactMatch;
            }

            return deviceNames
                .OrderByDescending(deviceName => deviceName.Length)
                .FirstOrDefault(deviceName =>
                    deviceName.Contains(cleanedValue, StringComparison.OrdinalIgnoreCase) ||
                    cleanedValue.Contains(deviceName, StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildAlarmSnapshotResponse(AlarmSnapshot snapshot, string alarmType)
        {
            if (snapshot.TotalCount == 0)
            {
                return alarmType == "critical alarms"
                    ? "There are no active alarms on critical-priority devices right now."
                    : "There are no active alarms right now.";
            }

            var builder = new StringBuilder();
            var verb = snapshot.TotalCount == 1 ? "is" : "are";
            var totalLabel = snapshot.TotalCount == 1 ? "alarm" : "alarms";
            var scope = alarmType == "critical alarms" ? " on critical-priority devices" : string.Empty;

            builder.AppendLine($"There {verb} {snapshot.TotalCount} active {totalLabel}{scope}.");
            builder.AppendLine($"Showing the latest {snapshot.Alarms.Count}:");

            foreach (var alarm in snapshot.Alarms)
            {
                var deviceDetails = alarm.DeviceFound
                    ? $"{alarm.DeviceName} (Device {alarm.DeviceId}) | {alarm.AlarmType} | Status {alarm.DeviceStatus} | Priority {alarm.PriorityLevel}"
                    : $"Device {alarm.DeviceId} (device details unavailable) | {alarm.AlarmType}";

                builder.AppendLine(
                    $"- Alarm {alarm.AlarmId} on {deviceDetails} | Raised {FormatUtc(alarm.RaisedTime)}");
            }

            if (snapshot.TotalCount > snapshot.Alarms.Count)
            {
                var remainingCount = snapshot.TotalCount - snapshot.Alarms.Count;
                var remainingLabel = remainingCount == 1 ? "alarm" : "alarms";
                builder.AppendLine($"... {remainingCount} more active {remainingLabel} not shown.");
            }

            return builder.ToString().TrimEnd();
        }

        private static string BuildDeviceStatusResponse(DeviceStatusSummary device)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Device status for {device.DeviceName}:");
            builder.AppendLine($"- Status: {device.Status}");
            builder.AppendLine($"- Type: {device.DeviceType}");
            builder.AppendLine($"- Priority: {device.PriorityLevel}");
            builder.AppendLine($"- IP: {device.IP}");
            builder.AppendLine($"- LEA ID: {device.LEAId}");
            builder.AppendLine($"- Simulated down: {(device.IsSimulatedDown ? "Yes" : "No")}");
            builder.AppendLine($"- Active alarms: {device.ActiveAlarmCount}");
            builder.AppendLine($"- Latest alarm: {(device.LatestAlarmRaisedTime.HasValue ? FormatUtc(device.LatestAlarmRaisedTime.Value) : "None")}");
            return builder.ToString().TrimEnd();
        }

        private static string BuildRootCauseResponse(RootCauseSummary rootCause)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Root cause for alarm {rootCause.AlarmId}:");
            builder.AppendLine($"- Root cause ID: {rootCause.RootCauseId}");
            builder.AppendLine($"- Device: {rootCause.RootCauseDeviceName} (Device {rootCause.RootCauseDeviceId})");
            builder.AppendLine($"- Type: {rootCause.RootCauseType}");
            builder.AppendLine($"- Detected: {FormatUtc(rootCause.DetectedTime)}");
            return builder.ToString().TrimEnd();
        }

        private static string BuildImpactedDevicesResponse(int alarmId, IReadOnlyList<ImpactedDeviceSummary> impactedDevices)
        {
            var builder = new StringBuilder();
            var preview = impactedDevices.Take(ImpactedDevicePreviewLimit).ToList();
            var verb = impactedDevices.Count == 1 ? "is" : "are";
            var deviceLabel = impactedDevices.Count == 1 ? "device" : "devices";

            builder.AppendLine($"There {verb} {impactedDevices.Count} impacted {deviceLabel} for alarm ID {alarmId}.");
            builder.AppendLine($"Showing the first {preview.Count}:");

            foreach (var impactedDevice in preview)
            {
                builder.AppendLine(
                    $"- {impactedDevice.DeviceName} (Device {impactedDevice.DeviceId}) | Type {impactedDevice.DeviceType} | Status {impactedDevice.Status} | Impact {impactedDevice.ImpactType}");
            }

            if (impactedDevices.Count > preview.Count)
            {
                var remainingCount = impactedDevices.Count - preview.Count;
                var remainingLabel = remainingCount == 1 ? "device" : "devices";
                builder.AppendLine($"... {remainingCount} more impacted {remainingLabel} not shown.");
            }

            return builder.ToString().TrimEnd();
        }

        private static string BuildDeviceSnapshotResponse(DeviceSnapshot snapshot, string label)
        {
            if (snapshot.TotalCount == 0)
            {
                return $"There are no {label} right now.";
            }

            var builder = new StringBuilder();
            var verb = snapshot.TotalCount == 1 ? "is" : "are";

            builder.AppendLine($"There {verb} {snapshot.TotalCount} {label}.");
            builder.AppendLine($"Showing the first {snapshot.Devices.Count}:");

            foreach (var device in snapshot.Devices)
            {
                builder.AppendLine(
                    $"- {device.DeviceName} (Device {device.DeviceId}) | Type {device.DeviceType} | Status {device.Status} | Priority {device.PriorityLevel} | IP {device.IP}");
            }

            if (snapshot.TotalCount > snapshot.Devices.Count)
            {
                var remainingCount = snapshot.TotalCount - snapshot.Devices.Count;
                builder.AppendLine($"... {remainingCount} more not shown.");
            }

            return builder.ToString().TrimEnd();
        }

        private static string FormatUtc(DateTime value)
        {
            return $"{value:yyyy-MM-dd HH:mm:ss} UTC";
        }

        private static string NormalizeLookupValue(string rawValue)
        {
            return CleanupExtractedValue(rawValue).ToLowerInvariant();
        }

        public sealed record AlarmSummary(
            int AlarmId,
            int DeviceId,
            string DeviceName,
            string AlarmType,
            DateTime RaisedTime,
            DeviceStatus DeviceStatus,
            PriorityLevel PriorityLevel,
            bool DeviceFound = true);

        public sealed record ActiveNodeSummary(
            int TotalNodes,
            int ActiveNodes,
            int DownNodes);

        public sealed record DeviceStatusSummary(
            int DeviceId,
            string DeviceName,
            string DeviceType,
            DeviceStatus Status,
            PriorityLevel PriorityLevel,
            string IP,
            int LEAId,
            bool IsSimulatedDown,
            int ActiveAlarmCount,
            DateTime? LatestAlarmRaisedTime);

        public sealed record RootCauseSummary(
            int RootCauseId,
            int AlarmId,
            int RootCauseDeviceId,
            string RootCauseDeviceName,
            string RootCauseType,
            DateTime DetectedTime);

        public sealed record ImpactedDeviceSummary(
            int ImpactId,
            int RootCauseId,
            int DeviceId,
            string DeviceName,
            string DeviceType,
            DeviceStatus Status,
            string ImpactType);

        private sealed record DeviceSummary(
            int DeviceId,
            string DeviceName,
            string DeviceType,
            DeviceStatus Status,
            PriorityLevel PriorityLevel,
            string IP);

        private sealed record DeviceSnapshot(
            int TotalCount,
            IReadOnlyList<DeviceSummary> Devices);

        private sealed record AlarmSnapshot(
            int TotalCount,
            IReadOnlyList<AlarmSummary> Alarms);

        private sealed class OllamaResponse
        {
            public string Response { get; set; } = string.Empty;
        }

        private enum ChatIntent
        {
            General,
            TotalNodes,
            ActiveNodes,
            DownNodes,
            ActiveAlarms,
            NetworkOverview,
            DeviceStatus,
            CriticalAlarms,
            RootCause,
            ImpactedDevices
        }
    }
}
