using INMS.Application.DTOs;
using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Enums;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INMS.Application.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly AppDbContext _context;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IUserAreaAssignmentRepository _assignmentRepository;

        public DeviceService(
            AppDbContext context,
            IDeviceRepository deviceRepository,
            IUserAreaAssignmentRepository assignmentRepository)
        {
            _context = context;
            _deviceRepository = deviceRepository;
            _assignmentRepository = assignmentRepository;
        }

        public async Task<IEnumerable<Device>> GetAllAsync()
        {
            return await _deviceRepository.GetAllAsync();
        }

        public async Task<IEnumerable<DeviceMapDto>> GetDevicesForMapAsync()
        {
            var devices = await _context.Devices
                .AsNoTracking()
                .Select(d => new
                {
                    d.DeviceId,
                    d.DeviceName,
                    d.DeviceType,
                    d.Latitude,
                    d.Longitude,
                    d.Status
                })
                .ToListAsync();

            var links = await _context.DeviceLinks
                .AsNoTracking()
                .Select(l => new { l.ParentDeviceId, l.ChildDeviceId })
                .ToListAsync();

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

            var impactedByDownRoots = new HashSet<int>();
            var downRootIds = devices
                .Where(d => d.Status == DeviceStatus.DOWN)
                .Select(d => d.DeviceId)
                .ToList();

            foreach (var rootId in downRootIds)
            {
                var visited = new HashSet<int> { rootId };
                var queue = new Queue<int>();
                queue.Enqueue(rootId);

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

                        impactedByDownRoots.Add(childId);
                        queue.Enqueue(childId);
                    }
                }
            }

            return devices.Select(d => new DeviceMapDto(
                d.DeviceId,
                d.DeviceName,
                d.DeviceType.ToString(),
                d.Latitude,
                d.Longitude,
                d.Status.ToString(),
                d.Status == DeviceStatus.DOWN ? 0 : (impactedByDownRoots.Contains(d.DeviceId) ? 1 : 0)
            ));
        }

        public async Task<Device?> GetByIdAsync(int id) => await _deviceRepository.GetByIdAsync(id);

        public async Task<Device> CreateAsync(CreateDeviceDto dto)
        {
            var device = new Device
            {
                DeviceName = dto.DeviceName,
                DeviceType = dto.DeviceType,
                IP = dto.IP ?? string.Empty,
                PriorityLevel = dto.PriorityLevel,
                LEAId = dto.LEAId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Status = DeviceStatus.UP
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<Device?> UpdateAsync(int id, UpdateDeviceDto dto)
        {
            var existing = await _deviceRepository.GetByIdAsync(id);
            if (existing == null) return null;

            var shouldPropagateImpact = string.Equals(dto.Status, "DOWN", StringComparison.OrdinalIgnoreCase);

            existing.DeviceName = dto.DeviceName;
            existing.DeviceType = dto.DeviceType;
            existing.IP = dto.IP ?? string.Empty;
            existing.Status = Enum.TryParse<DeviceStatus>(dto.Status, true, out var parsedStatus) ? parsedStatus : existing.Status;
            existing.PriorityLevel = dto.PriorityLevel;
            existing.LEAId = dto.LEAId;
            existing.Latitude = dto.Latitude;
            existing.Longitude = dto.Longitude;

            await _deviceRepository.UpdateAsync(existing);

            if (shouldPropagateImpact)
            {
                await PropagateImpact(id);
            }

            return existing;
        }

        public async Task PropagateImpact(int rootDeviceId)
        {
            var allLinks = await _context.DeviceLinks
                .AsNoTracking()
                .ToListAsync();

            var impactedDeviceIds = GetDownstreamDeviceIds(rootDeviceId, allLinks);
            var rootCauseId = await EnsureRootCauseAsync(rootDeviceId);

            // Rebuild the impacted rows for this root device to keep records current.
            var existingRows = await _context.ImpactedDevices
                .Where(x => x.RootCauseId == rootCauseId)
                .ToListAsync();

            if (existingRows.Count > 0)
            {
                _context.ImpactedDevices.RemoveRange(existingRows);
            }

            if (impactedDeviceIds.Count == 0)
            {
                await _context.SaveChangesAsync();
                return;
            }

            var impactedRows = impactedDeviceIds
                .Select(deviceId => new ImpactedDevice
                {
                    RootCauseId = rootCauseId,
                    DeviceId = deviceId,
                    ImpactType = "DOWNSTREAM"
                })
                .ToList();

            await _context.ImpactedDevices.AddRangeAsync(impactedRows);
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

        private static HashSet<int> GetDownstreamDeviceIds(
            int rootDeviceId,
            IEnumerable<DeviceLink> links)
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

        public async Task<bool> DeleteAsync(int id)
        {
            var device = await _deviceRepository.GetByIdAsync(id);
            if (device == null) return false;

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AssignDeviceAsync(int deviceId, int userId)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null) throw new Exception("Device not found");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found");

            device.AssignedUserId = userId;
            await _deviceRepository.UpdateAsync(device);
        }

        public async Task<List<Device>> GetVisibleDevicesAsync(int userId)
        {
            var assignment = await _assignmentRepository.GetByUserId(userId);

            if (assignment == null)
                return new List<Device>();

            return assignment.AreaType switch
            {
                "LEA"      => await _deviceRepository.GetDevicesByLeaAsync(assignment.AreaId),
                "Province" => await _deviceRepository.GetDevicesByProvinceAsync(assignment.AreaId),
                "Region"   => await _deviceRepository.GetDevicesByRegionAsync(assignment.AreaId),
                _          => new List<Device>()
            };
        }
        public async Task<Device?> UpdateStatusAsync(int id, DeviceStatus status)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return null;

            device.Status = status;
            await _context.SaveChangesAsync();
            return device;
        }
    }
}
