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
        private const int OllamaRequestTimeoutSeconds = 20;
        private const string GeneralSystemPrompt = @"You are a Network Operations Assistant for an Integrated Network Management System.
You understand SLBN, CEAN, and MSAN layers.
Answer only network-related questions.
If the question needs live data, guide the user toward supported queries such as total nodes, active nodes, down nodes, active alarms, device status, critical alarms, root cause, or impacted devices.
Be concise and technical.";
        private const int AlarmPreviewLimit = 10;
        private const int ImpactedDevicePreviewLimit = 10;

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
                join device in _dbContext.Devices.AsNoTracking() on alarm.DeviceId equals device.DeviceId
                where alarm.IsActive
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
            var activeNodeSummary = await GetActiveNodeCountAsync();
            return $"{activeNodeSummary.ActiveNodes} of {activeNodeSummary.TotalNodes} network nodes are currently active. {activeNodeSummary.DownNodes} are down or unreachable.";
        }

        private async Task<string> HandleDownNodesIntentAsync(string userMessage)
        {
            var nodeSummary = await GetActiveNodeCountAsync();
            return $"{nodeSummary.DownNodes} network nodes are currently down or unreachable. {nodeSummary.ActiveNodes} remain active out of {nodeSummary.TotalNodes} total nodes.";
        }

        private async Task<string> HandleActiveAlarmsIntentAsync(string userMessage)
        {
            var snapshot = await GetAlarmSnapshotAsync(criticalOnly: false, AlarmPreviewLimit);
            return BuildAlarmSnapshotResponse(snapshot, "active alarms");
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
                join device in _dbContext.Devices.AsNoTracking() on alarm.DeviceId equals device.DeviceId
                where alarm.IsActive && (!criticalOnly || device.PriorityLevel == PriorityLevel.Critical)
                select new
                {
                    alarm.AlarmId,
                    alarm.DeviceId,
                    device.DeviceName,
                    alarm.AlarmType,
                    alarm.RaisedTime,
                    device.Status,
                    device.PriorityLevel
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
                        row.DeviceName,
                        row.AlarmType,
                        row.RaisedTime,
                        row.Status,
                        row.PriorityLevel))
                    .ToList());
        }

        private async Task<string> RequestOllamaResponseAsync(string prompt)
        {
            var requestBody = new
            {
                model = OllamaModel,
                prompt,
                stream = false,
                keep_alive = "15m"
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
                    "number of nodes",
                    "node count",
                    "count of nodes"))
            {
                return ChatIntent.TotalNodes;
            }

            if (ContainsAny(normalized,
                    "active nodes",
                    "reachable nodes",
                    "active node count",
                    "up nodes",
                    "online nodes",
                    "healthy nodes",
                    "available nodes",
                    "how many are up",
                    "how many up"))
            {
                return ChatIntent.ActiveNodes;
            }

            if (ContainsAny(normalized,
                    "down nodes",
                    "failed nodes",
                    "unreachable nodes",
                    "offline nodes",
                    "how many are down",
                    "how many down"))
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
                    "current alarms",
                    "open alarms",
                    "show alarms",
                    "list alarms",
                    "alarm list",
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
                builder.AppendLine(
                    $"- AlarmId={alarm.AlarmId}; Device={alarm.DeviceName}; DeviceId={alarm.DeviceId}; AlarmType={alarm.AlarmType}; DeviceStatus={alarm.DeviceStatus}; Priority={alarm.PriorityLevel}; RaisedTimeUtc={alarm.RaisedTime:O}");
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
                builder.AppendLine(
                    $"- AlarmId={alarm.AlarmId}; Device={alarm.DeviceName}; DeviceId={alarm.DeviceId}; AlarmType={alarm.AlarmType}; DeviceStatus={alarm.DeviceStatus}; Priority={alarm.PriorityLevel}; RaisedTimeUtc={alarm.RaisedTime:O}");
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
                builder.AppendLine(
                    $"- Alarm {alarm.AlarmId} on {alarm.DeviceName} (Device {alarm.DeviceId}) | {alarm.AlarmType} | Status {alarm.DeviceStatus} | Priority {alarm.PriorityLevel} | Raised {FormatUtc(alarm.RaisedTime)}");
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
            PriorityLevel PriorityLevel);

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
            DeviceStatus,
            CriticalAlarms,
            RootCause,
            ImpactedDevices
        }
    }
}
