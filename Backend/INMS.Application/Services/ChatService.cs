using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
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
        private const string DatabaseAwareSystemPrompt = @"You are a Network Operations Assistant for an Integrated Network Management System.
Use only the live database facts provided in the context section.
Do not invent alarms, devices, root causes, or impacted devices.
If the database says no records were found, say that clearly.
Answer in concise, technical, operator-friendly language.";
        private const string GeneralSystemPrompt = @"You are a Network Operations Assistant for an Integrated Network Management System.
You understand SLBN, CEAN, and MSAN layers.
Answer only network-related questions.
If the question needs live data, guide the user toward supported queries such as active alarms, device status, critical alarms, root cause, or impacted devices.
Be concise and technical.";

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

        public ChatService(HttpClient httpClient, AppDbContext dbContext, ILogger<ChatService> logger)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Please enter a message so I can help.";
            }

            var trimmedMessage = userMessage.Trim();
            var intent = DetectIntent(trimmedMessage);

            try
            {
                return intent switch
                {
                    ChatIntent.ActiveAlarms => await HandleActiveAlarmsIntentAsync(trimmedMessage),
                    ChatIntent.DeviceStatus => await HandleDeviceStatusIntentAsync(trimmedMessage),
                    ChatIntent.CriticalAlarms => await HandleCriticalAlarmsIntentAsync(trimmedMessage),
                    ChatIntent.RootCause => await HandleRootCauseIntentAsync(trimmedMessage),
                    ChatIntent.ImpactedDevices => await HandleImpactedDevicesIntentAsync(trimmedMessage),
                    _ => await GenerateGeneralResponseAsync(trimmedMessage)
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while processing chat request: {UserMessage}", trimmedMessage);
                return "I couldn't read the latest network data because the database operation failed. Please try again.";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database read error while processing chat request: {UserMessage}", trimmedMessage);
                return "I couldn't process the live database results for that request. Please try again.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ollama request failed while processing chat request: {UserMessage}", trimmedMessage);
                return "I couldn't reach the local Ollama service. Please make sure Ollama is running and the llama3 model is available.";
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Ollama request timed out while processing chat request: {UserMessage}", trimmedMessage);
                return "The AI response timed out. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing chat request: {UserMessage}", trimmedMessage);
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

        public async Task<DeviceStatusSummary?> GetDeviceStatusAsync(string deviceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                return null;
            }

            var normalizedDeviceName = NormalizeLookupValue(deviceName);

            var exactMatch = await _dbContext.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(device => device.DeviceName.ToLower() == normalizedDeviceName);

            var matchedDevice = exactMatch;

            if (matchedDevice == null)
            {
                matchedDevice = await _dbContext.Devices
                    .AsNoTracking()
                    .Where(device =>
                        device.DeviceName.ToLower().Contains(normalizedDeviceName) ||
                        normalizedDeviceName.Contains(device.DeviceName.ToLower()))
                    .OrderBy(device => device.DeviceName.Length)
                    .FirstOrDefaultAsync();
            }

            if (matchedDevice == null)
            {
                return null;
            }

            var activeAlarmCount = await _dbContext.Alarms
                .AsNoTracking()
                .CountAsync(alarm => alarm.DeviceId == matchedDevice.DeviceId && alarm.IsActive);

            var latestAlarmRaisedTime = await _dbContext.Alarms
                .AsNoTracking()
                .Where(alarm => alarm.DeviceId == matchedDevice.DeviceId)
                .OrderByDescending(alarm => alarm.RaisedTime)
                .Select(alarm => (DateTime?)alarm.RaisedTime)
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
                activeAlarmCount,
                latestAlarmRaisedTime);
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

        private async Task<string> HandleActiveAlarmsIntentAsync(string userMessage)
        {
            var alarms = await GetActiveAlarmsAsync();
            var structuredData = BuildActiveAlarmsContext(alarms);
            return await GenerateDatabaseAwareResponseAsync(userMessage, "active alarms", structuredData);
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

            var structuredData = BuildDeviceStatusContext(device);
            return await GenerateDatabaseAwareResponseAsync(userMessage, "device status", structuredData);
        }

        private async Task<string> HandleCriticalAlarmsIntentAsync(string userMessage)
        {
            var alarms = await GetCriticalAlarmsAsync();
            var structuredData = BuildCriticalAlarmsContext(alarms);
            return await GenerateDatabaseAwareResponseAsync(userMessage, "critical alarms", structuredData);
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

            var structuredData = BuildRootCauseContext(rootCause);
            return await GenerateDatabaseAwareResponseAsync(userMessage, "root cause", structuredData);
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

            var structuredData = BuildImpactedDevicesContext(alarmId.Value, impactedDevices);
            return await GenerateDatabaseAwareResponseAsync(userMessage, "impacted devices", structuredData);
        }

        private async Task<string> GenerateGeneralResponseAsync(string userMessage)
        {
            var prompt = $"{GeneralSystemPrompt}\n\nUser: {userMessage}\nAssistant:";
            return await RequestOllamaResponseAsync(prompt);
        }

        private async Task<string> GenerateDatabaseAwareResponseAsync(string userMessage, string intentName, string structuredData)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine(DatabaseAwareSystemPrompt);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine($"Intent: {intentName}");
            promptBuilder.AppendLine("Database context:");
            promptBuilder.AppendLine(structuredData);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine($"User: {userMessage}");
            promptBuilder.AppendLine("Assistant:");

            return await RequestOllamaResponseAsync(promptBuilder.ToString());
        }

        private async Task<string> RequestOllamaResponseAsync(string prompt)
        {
            var requestBody = new
            {
                model = OllamaModel,
                prompt,
                stream = false
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

            if (normalized.Contains("impacted devices"))
            {
                return ChatIntent.ImpactedDevices;
            }

            if (normalized.Contains("root cause"))
            {
                return ChatIntent.RootCause;
            }

            if (normalized.Contains("device status") || normalized.Contains("status of") || normalized.Contains("status for"))
            {
                return ChatIntent.DeviceStatus;
            }

            if (normalized.Contains("active alarms") || normalized.Contains("current alarms"))
            {
                return ChatIntent.ActiveAlarms;
            }

            if (normalized.Contains("critical"))
            {
                return ChatIntent.CriticalAlarms;
            }

            return ChatIntent.General;
        }

        private async Task<string?> ExtractDeviceNameAsync(string userMessage)
        {
            var quotedMatch = Regex.Match(userMessage, "[\"'](?<value>[^\"']+)[\"']", RegexOptions.IgnoreCase);
            if (quotedMatch.Success)
            {
                return quotedMatch.Groups["value"].Value.Trim();
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
                    return candidate;
                }
            }

            var deviceNames = await _dbContext.Devices
                .AsNoTracking()
                .Select(device => device.DeviceName)
                .ToListAsync();

            return deviceNames
                .OrderByDescending(deviceName => deviceName.Length)
                .FirstOrDefault(deviceName => userMessage.Contains(deviceName, StringComparison.OrdinalIgnoreCase));
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

        private sealed class OllamaResponse
        {
            public string Response { get; set; } = string.Empty;
        }

        private enum ChatIntent
        {
            General,
            ActiveAlarms,
            DeviceStatus,
            CriticalAlarms,
            RootCause,
            ImpactedDevices
        }
    }
}
