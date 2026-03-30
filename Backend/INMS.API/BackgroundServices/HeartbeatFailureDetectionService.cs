using INMS.Application.Interfaces;
using INMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
        var context = scope.ServiceProvider.GetRequiredService<INMS.Infrastructure.Persistence.AppDbContext>();
        var simulationEventService = scope.ServiceProvider.GetRequiredService<ISimulationEventService>();
            var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceService>();
        var currentTime = DateTime.UtcNow;

        var devices = await context.Devices.ToListAsync();

        var deviceLinks = await context.DeviceLinks
            .AsNoTracking()
            .ToListAsync();

        var deviceIds = devices.Select(d => d.DeviceId).ToList();
        var latestHeartbeats = await context.Heartbeats
            .Where(h => deviceIds.Contains(h.DeviceId))
            .GroupBy(h => h.DeviceId)
            .Select(g => g.OrderByDescending(h => h.Timestamp).First())
            .ToDictionaryAsync(h => h.DeviceId);

        var parentIdsByChild = deviceLinks
            .GroupBy(dl => dl.ChildDeviceId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ParentDeviceId).Distinct().ToList());

        var baseStatusByDevice = new Dictionary<int, DeviceStatus>(devices.Count);
        foreach (var device in devices)
        {
            if (device.IsSimulatedDown)
            {
                baseStatusByDevice[device.DeviceId] = DeviceStatus.DOWN;
                continue;
            }

            latestHeartbeats.TryGetValue(device.DeviceId, out var lastHeartbeat);

            if (lastHeartbeat == null)
            {
                baseStatusByDevice[device.DeviceId] = DeviceStatus.DOWN;
                continue;
            }

            var timeSinceLast = (currentTime - lastHeartbeat.Timestamp).TotalSeconds;
            baseStatusByDevice[device.DeviceId] = timeSinceLast > FailureTimeoutSeconds
                ? DeviceStatus.DOWN
                : DeviceStatus.UP;
        }

        var computedStatusByDevice = new Dictionary<int, DeviceStatus>(baseStatusByDevice);
        bool hasChanges;

        do
        {
            hasChanges = false;

            foreach (var device in devices)
            {
                var currentStatus = computedStatusByDevice[device.DeviceId];
                if (currentStatus == DeviceStatus.DOWN)
                {
                    continue;
                }

                if (!parentIdsByChild.TryGetValue(device.DeviceId, out var parentIds) || parentIds.Count == 0)
                {
                    var desiredRootStatus = baseStatusByDevice[device.DeviceId];
                    if (currentStatus != desiredRootStatus)
                    {
                        computedStatusByDevice[device.DeviceId] = desiredRootStatus;
                        hasChanges = true;
                    }
                    continue;
                }

                bool allParentsUnavailable = parentIds.All(parentId =>
                    computedStatusByDevice.TryGetValue(parentId, out var parentStatus) &&
                    (parentStatus == DeviceStatus.DOWN || parentStatus == DeviceStatus.UNREACHABLE));

                var desiredStatus = allParentsUnavailable
                    ? DeviceStatus.UNREACHABLE
                    : DeviceStatus.UP;

                if (currentStatus != desiredStatus)
                {
                    computedStatusByDevice[device.DeviceId] = desiredStatus;
                    hasChanges = true;
                }
            }
        } while (hasChanges);

            foreach (var device in devices)
        {
            var oldStatus = device.Status;
            var resolvedStatus = computedStatusByDevice[device.DeviceId];

            if (oldStatus == resolvedStatus)
            {
                continue;
            }

            device.Status = resolvedStatus;

            if (device.IsSimulatedDown && resolvedStatus == DeviceStatus.DOWN && oldStatus != DeviceStatus.DOWN)
            {
                await simulationEventService.LogEventAsync(device.DeviceId, "SIMULATED_DOWN");
                _logger.LogWarning($"Device {device.DeviceId} forced DOWN because IsSimulatedDown=true");
                continue;
            }

                if (resolvedStatus == DeviceStatus.DOWN && oldStatus != DeviceStatus.DOWN)
                {
                    await simulationEventService.LogEventAsync(device.DeviceId, "HEARTBEAT_TIMEOUT");
                    _logger.LogWarning($"Device {device.DeviceId} marked OFFLINE due to heartbeat timeout");

                    // Trigger downstream impact propagation and alarm creation for a true state change to DOWN
                    await deviceService.PropagateImpact(device.DeviceId);
                }
            else if (resolvedStatus == DeviceStatus.UP && oldStatus != DeviceStatus.UP)
            {
                await simulationEventService.LogEventAsync(device.DeviceId, "HEARTBEAT_RECOVERED");
                _logger.LogInformation($"Device {device.DeviceId} recovered and marked ONLINE");
            }
                else if (resolvedStatus == DeviceStatus.UNREACHABLE && oldStatus != DeviceStatus.UNREACHABLE)
                {
                    _logger.LogInformation($"Device {device.DeviceId} marked UNREACHABLE due to upstream dependency");

                    // Ensure an unreachable alarm and impacted devices are recorded for this state change
                    await deviceService.EnsureUnreachableAlarmAsync(device.DeviceId);
                }
        }

        await context.SaveChangesAsync();
    }
}
