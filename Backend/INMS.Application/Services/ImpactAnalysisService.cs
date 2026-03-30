using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Enums;
using INMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INMS.Application.Services;

public class ImpactAnalysisService : IImpactAnalysisService
{
    private readonly AppDbContext _context;

    public ImpactAnalysisService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AnalyzeFailureAsync(int deviceId)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId);

        if (device == null || device.Status != nameof(DeviceStatus.DOWN))
        {
            return;
        }

        var links = await _context.DeviceLinks
            .Include(dl => dl.ParentDevice)
            .ToListAsync();

        var parentLinks = links
            .Where(link => link.ChildDeviceId == deviceId)
            .ToList();

        var isRootFailure = parentLinks.Count == 0 ||
            parentLinks.All(link => link.ParentDevice != null && link.ParentDevice.Status == nameof(DeviceStatus.UP));

        if (!isRootFailure)
        {
            await ClearRootCauseAsync(deviceId);
            return;
        }

        var rootCauseId = await EnsureRootCauseAsync(deviceId);
        var impactedDeviceIds = GetDownstreamDeviceIds(deviceId, links);

        var existingRows = await _context.ImpactedDevices
            .Where(x => x.RootCauseId == rootCauseId)
            .ToListAsync();

        if (existingRows.Count > 0)
        {
            _context.ImpactedDevices.RemoveRange(existingRows);
        }

        if (impactedDeviceIds.Count > 0)
        {
            var impactedRows = impactedDeviceIds
                .Select(impactedDeviceId => new ImpactedDevice
                {
                    RootCauseId = rootCauseId,
                    DeviceId = impactedDeviceId,
                    ImpactType = "DOWNSTREAM"
                })
                .ToList();

            await _context.ImpactedDevices.AddRangeAsync(impactedRows);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ClearImpactAsync(int deviceId)
    {
        await ClearRootCauseAsync(deviceId);

        var directImpactRows = await _context.ImpactedDevices
            .Where(id => id.DeviceId == deviceId)
            .ToListAsync();

        if (directImpactRows.Count > 0)
        {
            _context.ImpactedDevices.RemoveRange(directImpactRows);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ClearRootCauseAsync(int deviceId)
    {
        var rootCauseIds = await _context.RootCauses
            .Where(rc => rc.RootCauseDeviceId == deviceId)
            .Select(rc => rc.RootCauseId)
            .ToListAsync();

        if (rootCauseIds.Count > 0)
        {
            var impactedRows = await _context.ImpactedDevices
                .Where(id => rootCauseIds.Contains(id.RootCauseId))
                .ToListAsync();

            if (impactedRows.Count > 0)
            {
                _context.ImpactedDevices.RemoveRange(impactedRows);
            }

            var rootCauses = await _context.RootCauses
                .Where(rc => rootCauseIds.Contains(rc.RootCauseId))
                .ToListAsync();

            _context.RootCauses.RemoveRange(rootCauses);
        }

        await _context.SaveChangesAsync();
    }

    private async Task<int> EnsureRootCauseAsync(int rootDeviceId)
    {
        var activeAlarm = await _context.Alarms
            .Where(a => a.DeviceId == rootDeviceId && a.IsActive && a.AlarmType == "NODE_DOWN")
            .OrderByDescending(a => a.RaisedTime)
            .FirstOrDefaultAsync();

        if (activeAlarm == null)
        {
            activeAlarm = new Alarm
            {
                DeviceId = rootDeviceId,
                AlarmType = "NODE_DOWN",
                RaisedTime = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Alarms.AddAsync(activeAlarm);
            await _context.SaveChangesAsync();
        }

        var rootCause = await _context.RootCauses
            .Where(rc => rc.AlarmId == activeAlarm.AlarmId)
            .OrderByDescending(rc => rc.DetectedTime)
            .FirstOrDefaultAsync();

        if (rootCause == null)
        {
            rootCause = new RootCause
            {
                AlarmId = activeAlarm.AlarmId,
                RootCauseDeviceId = rootDeviceId,
                RootCauseType = "NODE_FAILURE",
                DetectedTime = DateTime.UtcNow
            };

            await _context.RootCauses.AddAsync(rootCause);
            await _context.SaveChangesAsync();
        }

        return rootCause.RootCauseId;
    }

    private static HashSet<int> GetDownstreamDeviceIds(int rootDeviceId, IEnumerable<DeviceLink> links)
    {
        var adjacency = new Dictionary<int, List<int>>();

        foreach (var link in links)
        {
            if (!adjacency.TryGetValue(link.ParentDeviceId, out var children))
            {
                children = new List<int>();
                adjacency[link.ParentDeviceId] = children;
            }

            children.Add(link.ChildDeviceId);
        }

        var visited = new HashSet<int> { rootDeviceId };
        var impacted = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(rootDeviceId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (!adjacency.TryGetValue(current, out var children))
            {
                continue;
            }

            foreach (var childId in children)
            {
                if (!visited.Add(childId))
                {
                    continue;
                }

                impacted.Add(childId);
                queue.Enqueue(childId);
            }
        }

        return impacted;
    }
}
