using INMS.Application.Interfaces;
using INMS.Domain.Enums;
using INMS.Infrastructure.Persistence;
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
        var currentTime = DateTime.UtcNow;

        // Load all devices
        var devices = await context.Devices.ToListAsync();

        // Load all DeviceLinks with parent devices for multi-parent topology
        var deviceLinks = await context.DeviceLinks
            .Include(dl => dl.ParentDevice)
            .Include(dl => dl.ChildDevice)
            .ToListAsync();

        // OPTIMIZATION: Load latest heartbeats in single query to avoid N+1
        var deviceIds = devices.Select(d => d.DeviceId).ToList();
        var latestHeartbeats = await context.Heartbeats
            .Where(h => deviceIds.Contains(h.DeviceId))
            .GroupBy(h => h.DeviceId)
            .Select(g => g.OrderByDescending(h => h.Timestamp).First())
            .ToDictionaryAsync(h => h.DeviceId);

        // Group device links by child device for efficient lookup
        var parentLinksByChild = deviceLinks
            .GroupBy(dl => dl.ChildDeviceId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var device in devices)
        {
            DeviceStatus newStatus = DeviceStatus.UP;
            bool isTopologyBasedChange = false;

            // MULTI-PARENT TOPOLOGY: Check all parent devices via DeviceLink
            if (parentLinksByChild.TryGetValue(device.DeviceId, out var parentLinks) && parentLinks.Any())
            {
                // Check if ALL parents are DOWN or UNREACHABLE
                bool allParentsDown = parentLinks.All(link =>
                    link.ParentDevice != null &&
                    (link.ParentDevice.Status == DeviceStatus.DOWN ||
                     link.ParentDevice.Status == DeviceStatus.UNREACHABLE));

                if (allParentsDown)
                {
                    newStatus = DeviceStatus.UNREACHABLE;
                    isTopologyBasedChange = true;
                    _logger.LogInformation($"Device {device.DeviceId} marked UNREACHABLE (all {parentLinks.Count} parents are DOWN/UNREACHABLE)");
                }
                else
                {
                    // At least one parent is UP, proceed to heartbeat check
                    latestHeartbeats.TryGetValue(device.DeviceId, out var lastHeartbeat);

                    if (lastHeartbeat != null)
                    {
                        var timeSinceLast = (currentTime - lastHeartbeat.Timestamp).TotalSeconds;

                        if (timeSinceLast > FailureTimeoutSeconds)
                        {
                            newStatus = DeviceStatus.DOWN;
                        }
                        else
                        {
                            newStatus = DeviceStatus.UP;
                        }
                    }
                }
            }
            else
            {
                // No parents (root device), check heartbeat directly
                latestHeartbeats.TryGetValue(device.DeviceId, out var lastHeartbeat);

                if (lastHeartbeat != null)
                {
                    var timeSinceLast = (currentTime - lastHeartbeat.Timestamp).TotalSeconds;

                    if (timeSinceLast > FailureTimeoutSeconds)
                    {
                        newStatus = DeviceStatus.DOWN;
                    }
                    else
                    {
                        newStatus = DeviceStatus.UP;
                    }
                }
            }

            // Update status only if changed
            if (device.Status != newStatus)
            {
                var oldStatus = device.Status;
                device.Status = newStatus;

                // CORRECTED: Only log heartbeat events for real heartbeat failures/recoveries
                // Not for topology-based UNREACHABLE changes
                if (!isTopologyBasedChange)
                {
                    if (newStatus == DeviceStatus.DOWN && oldStatus != DeviceStatus.DOWN)
                    {
                        await simulationEventService.LogEventAsync(device.DeviceId, "HEARTBEAT_TIMEOUT");
                        _logger.LogWarning($"Device {device.DeviceId} marked OFFLINE due to heartbeat timeout");
                    }
                    else if (newStatus == DeviceStatus.UP && oldStatus == DeviceStatus.DOWN)
                    {
                        await simulationEventService.LogEventAsync(device.DeviceId, "HEARTBEAT_RECOVERED");
                        _logger.LogInformation($"Device {device.DeviceId} recovered and marked ONLINE");
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
