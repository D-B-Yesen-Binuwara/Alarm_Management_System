using INMS.Application.Interfaces;
using INMS.Domain.Enums;

namespace INMS.API.BackgroundServices;

public class HeartbeatSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HeartbeatSchedulerService> _logger;
    private const int HeartbeatIntervalSeconds = 30;

    public HeartbeatSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<HeartbeatSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Heartbeat Scheduler Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformHeartbeatCheck();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during heartbeat check");
            }

            await Task.Delay(TimeSpan.FromSeconds(HeartbeatIntervalSeconds), stoppingToken);
        }
    }

    private async Task PerformHeartbeatCheck()
    {
        using var scope = _serviceProvider.CreateScope();
        var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceService>();
        var heartbeatService = scope.ServiceProvider.GetRequiredService<IHeartbeatService>();

        var devices = await deviceService.GetAllAsync();
        int respondedCount = 0;

        foreach (var device in devices)
        {
            if (device.IsSimulatedDown)
            {
                _logger.LogDebug($"Device {device.DeviceId} is simulated down - skipping heartbeat");
                continue;
            }

            // Record heartbeat regardless of current status
            // This allows automatic recovery detection
            await heartbeatService.RecordHeartbeatAsync(device.DeviceId, "UP");
            respondedCount++;
            _logger.LogDebug($"Heartbeat recorded for device {device.DeviceId}");
        }

        _logger.LogInformation($"Heartbeat check completed: {respondedCount}/{devices.Count()} devices responded");
    }
}
