using INMS.Application.Interfaces;
using INMS.Domain.Enums;

namespace INMS.API.BackgroundServices;

public class HeartbeatSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HeartbeatSchedulerService> _logger;
    private const int HeartbeatIntervalSeconds = 30;
    private static readonly Random _random = new Random();

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
            if (device.Status == DeviceStatus.UP)
            {
                // Simulate 90% success rate (10% packet loss)
                bool responded = _random.Next(0, 100) > 10;

                if (responded)
                {
                    await heartbeatService.RecordHeartbeatAsync(device.DeviceId, "UP");
                    respondedCount++;
                    _logger.LogDebug($"Heartbeat recorded for device {device.DeviceId}");
                }
                else
                {
                    _logger.LogDebug($"Device {device.DeviceId} did not respond (simulated packet loss)");
                }
            }
        }

        _logger.LogInformation($"Heartbeat check completed: {respondedCount}/{devices.Count()} devices responded");
    }
}
