using INMS.Application.Interfaces;
using INMS.Domain.Enums;

namespace INMS.API.BackgroundServices;

public class HeartbeatFailureDetectionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HeartbeatFailureDetectionService> _logger;
    private const int CheckIntervalSeconds = 30;
    private const int FailureTimeoutSeconds = 90;

    public HeartbeatFailureDetectionService(
        IServiceProvider serviceProvider,
        ILogger<HeartbeatFailureDetectionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Heartbeat Failure Detection Service started");

        await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DetectFailures();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during failure detection");
            }

            await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), stoppingToken);
        }
    }

    private async Task DetectFailures()
    {
        using var scope = _serviceProvider.CreateScope();
        var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceService>();
        var heartbeatService = scope.ServiceProvider.GetRequiredService<IHeartbeatService>();
        var simulationEventService = scope.ServiceProvider.GetRequiredService<ISimulationEventService>();

        var devices = await deviceService.GetAllAsync();
        var currentTime = DateTime.Now;

        foreach (var device in devices)
        {
            var latestHeartbeat = await heartbeatService.GetLatestHeartbeatAsync(device.DeviceId);

            if (latestHeartbeat != null)
            {
                var timeSinceLastHeartbeat = (currentTime - latestHeartbeat.Timestamp).TotalSeconds;

                if (timeSinceLastHeartbeat > FailureTimeoutSeconds && device.Status != DeviceStatus.DOWN)
                {
                    await deviceService.UpdateStatusAsync(device.DeviceId, DeviceStatus.DOWN);
                    await simulationEventService.LogEventAsync(device.DeviceId, "HEARTBEAT_TIMEOUT");
                    
                    _logger.LogWarning($"Device {device.DeviceId} marked OFFLINE due to heartbeat timeout ({timeSinceLastHeartbeat}s)");
                }
                else if (timeSinceLastHeartbeat <= FailureTimeoutSeconds && device.Status == DeviceStatus.DOWN)
                {
                    await deviceService.UpdateStatusAsync(device.DeviceId, DeviceStatus.UP);
                    await simulationEventService.LogEventAsync(device.DeviceId, "HEARTBEAT_RECOVERED");
                    
                    _logger.LogInformation($"Device {device.DeviceId} recovered and marked ONLINE");
                }
            }
        }
    }
}
