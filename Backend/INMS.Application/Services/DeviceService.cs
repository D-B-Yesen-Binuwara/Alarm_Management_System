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
        private readonly ISimulationEventService _simulationEventService;
        private readonly IImpactAnalysisService _impactAnalysisService;

        public DeviceService(
            AppDbContext context,
            IDeviceRepository deviceRepository,
            IUserAreaAssignmentRepository assignmentRepository,
            ISimulationEventService simulationEventService,
            IImpactAnalysisService impactAnalysisService)
        {
            _context = context;
            _deviceRepository = deviceRepository;
            _assignmentRepository = assignmentRepository;
            _simulationEventService = simulationEventService;
            _impactAnalysisService = impactAnalysisService;
        }

        public async Task<IEnumerable<Device>> GetAllAsync(int? callerUserId = null)
        {
            var (role, layer) = await ResolveCallerAsync(callerUserId);

            if (!string.IsNullOrEmpty(role) && role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue)
            {
                return await _device_repository_GetAllFilteredByLayer(layer.Value);
            }

            return await _deviceRepository.GetAllAsync();
        }

        // helper to avoid loading everything into memory in callers that can use repository filtering later
        private async Task<IEnumerable<Device>> _device_repository_GetAllFilteredByLayer(Domain.Enums.DeviceType layer)
        {
            return await _deviceRepository.GetDevicesByDeviceTypeAsync(layer);
        }

        public async Task<IEnumerable<DeviceListDto>> GetAllForDashboardAsync(int? callerUserId = null)
        {
            var (role, layer) = await ResolveCallerAsync(callerUserId);
            var query = _context.Devices
                .AsNoTracking()
                .Join(_context.LEAs.AsNoTracking(),
                    d => d.LEAId,
                    l => l.LEAId,
                    (d, l) => new { Device = d, Lea = l })
                .Join(_context.Provinces.AsNoTracking(),
                    dl => dl.Lea.ProvinceId,
                    p => p.ProvinceId,
                    (dl, p) => new { dl.Device, dl.Lea, Province = p })
                .Join(_context.Regions.AsNoTracking(),
                    dlp => dlp.Province.RegionId,
                    r => r.RegionId,
                    (dlp, r) => new { dlp.Device, dlp.Lea, dlp.Province, Region = r })
                .GroupJoin(_context.Users.AsNoTracking(),
                    dlpr => dlpr.Device.AssignedUserId,
                    u => u.UserId,
                    (dlpr, users) => new { dlpr.Device, dlpr.Lea, dlpr.Province, dlpr.Region, Users = users });

            if (!string.IsNullOrEmpty(role) && role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue)
            {
                query = query.Where(x => x.Device.DeviceType == layer.Value);
            }

            var rows = await query
                .SelectMany(
                    x => x.Users.DefaultIfEmpty(),
                    (x, u) => new DeviceListDto(
                        x.Device.DeviceId,
                        x.Device.DeviceName,
                        x.Device.DeviceType,
                        x.Device.IP,
                        x.Device.Status,
                        x.Device.PriorityLevel,
                        x.Device.LEAId,
                        x.Lea.Name,
                        x.Province.Name,
                        x.Region.Name,
                        x.Device.Latitude,
                        x.Device.Longitude,
                        x.Device.AssignedUserId,
                        u != null ? u.FullName : null,
                        u != null ? u.ServiceId : null,
                        x.Device.IsSimulatedDown
                    ))
                .ToListAsync();

            return rows;
        }

        public async Task<IEnumerable<DeviceMapDto>> GetDevicesForMapAsync(int? callerUserId = null)
        {
            var (role, layer) = await ResolveCallerAsync(callerUserId);

            var devicesQuery = _context.Devices
                .AsNoTracking()
                .Select(d => new
                {
                    d.DeviceId,
                    d.DeviceName,
                    d.DeviceType,
                    d.Latitude,
                    d.Longitude,
                    d.Status
                });

            if (!string.IsNullOrEmpty(role) && role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue)
            {
                devicesQuery = devicesQuery.Where(d => d.DeviceType == layer.Value);
            }

            var devices = await devicesQuery.ToListAsync();

            var deviceIds = devices.Select(d => d.DeviceId).ToHashSet();

            var links = await _context.DeviceLinks
                .AsNoTracking()
                .Where(l => deviceIds.Contains(l.ParentDeviceId) || deviceIds.Contains(l.ChildDeviceId))
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

            var mapped = devices.Select(d => new DeviceMapDto(
                d.DeviceId,
                d.DeviceName,
                d.DeviceType.ToString(),
                d.Latitude,
                d.Longitude,
                d.Status.ToString(),
                d.Status == DeviceStatus.DOWN ? 0 : (impactedByDownRoots.Contains(d.DeviceId) ? 1 : 0)
            ));

            if (!string.IsNullOrEmpty(role) && role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue)
            {
                mapped = mapped.Where(m => Enum.TryParse<Domain.Enums.DeviceType>(m.DeviceType, out var dt) && dt == layer.Value);
            }

            return mapped;
        }

        // Resolve caller's role and layer from DB when callerUserId provided
        private async Task<(string role, Domain.Enums.DeviceType? layer)> ResolveCallerAsync(int? callerUserId)
        {
            if (!callerUserId.HasValue) return (string.Empty, null);
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == callerUserId.Value);
            if (user == null) return (string.Empty, null);
            return (user.Role?.RoleName ?? string.Empty, user.Layer);
        }

        public async Task<Device?> GetByIdAsync(int id, int? callerUserId = null)
        {
            var device = await _deviceRepository.GetByIdAsync(id);
            if (!callerUserId.HasValue) return device;

            var (role, layer) = await ResolveCallerAsync(callerUserId);
            if (string.IsNullOrEmpty(role)) return device;
            if (role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)) return device;
            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue && device != null && device.DeviceType != layer.Value)
            {
                throw new UnauthorizedAccessException("Caller not permitted to access this device");
            }

            return device;
        }

        public async Task<Device> CreateAsync(CreateDeviceDto dto, int? callerUserId = null)
        {
            if (callerUserId.HasValue)
            {
                var (role, layer) = await ResolveCallerAsync(callerUserId);
                if (!string.IsNullOrEmpty(role) && !role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue && dto.DeviceType != layer.Value)
                        throw new UnauthorizedAccessException("Caller cannot create devices outside their layer");
                }
            }

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

        public async Task<Device?> UpdateAsync(int id, UpdateDeviceDto dto, int? callerUserId = null)
        {
            var existing = await _deviceRepository.GetByIdAsync(id);
            if (existing == null) return null;

            if (callerUserId.HasValue)
            {
                var (role, layer) = await ResolveCallerAsync(callerUserId);
                if (!string.IsNullOrEmpty(role) && !role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue && existing.DeviceType != layer.Value)
                        throw new UnauthorizedAccessException("Caller cannot update this device");
                    if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue && dto.DeviceType != layer.Value)
                        throw new UnauthorizedAccessException("Caller cannot change device layer");
                }
            }

            existing.DeviceName = dto.DeviceName;
            existing.DeviceType = dto.DeviceType;
            existing.IP = dto.IP ?? string.Empty;
            existing.Status = Enum.TryParse<DeviceStatus>(dto.Status, true, out var parsedStatus) ? parsedStatus : existing.Status;
            existing.PriorityLevel = dto.PriorityLevel;
            existing.LEAId = dto.LEAId;
            existing.Latitude = dto.Latitude;
            existing.Longitude = dto.Longitude;

            await _deviceRepository.UpdateAsync(existing);

            return existing;
        }

        public async Task<bool> DeleteAsync(int id, int? callerUserId = null)
        {
            var device = await _deviceRepository.GetByIdAsync(id);
            if (device == null) return false;

            if (callerUserId.HasValue)
            {
                var (role, layer) = await ResolveCallerAsync(callerUserId);
                if (!string.IsNullOrEmpty(role) && !role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue && device.DeviceType != layer.Value)
                        throw new UnauthorizedAccessException("Caller cannot delete this device");
                }
            }

            // 1. Delete associated logs perfectly without memory overhead
            await _context.Heartbeats.Where(h => h.DeviceId == id).ExecuteDeleteAsync();
            await _context.SimulationEvents.Where(s => s.DeviceId == id).ExecuteDeleteAsync();

            // 2. Clear out topological ties
            await _context.DeviceLinks.Where(l => l.ParentDeviceId == id || l.ChildDeviceId == id).ExecuteDeleteAsync();

            // 3. Clear direct impact analysis & root causes
            await _context.ImpactedDevices.Where(i => i.DeviceId == id).ExecuteDeleteAsync();

            // 4. Clean up relational alarms
            var alarmIds = await _context.Alarms.Where(a => a.DeviceId == id).Select(a => a.AlarmId).ToListAsync();

            if (alarmIds.Count > 0)
            {
                var rcIds = await _context.RootCauses
                    .Where(rc => alarmIds.Contains(rc.AlarmId) || rc.RootCauseDeviceId == id)
                    .Select(rc => rc.RootCauseId)
                    .ToListAsync();
                
                if (rcIds.Count > 0)
                {
                    await _context.ImpactedDevices.Where(i => rcIds.Contains(i.RootCauseId)).ExecuteDeleteAsync();
                }
                
                await _context.RootCauses.Where(rc => alarmIds.Contains(rc.AlarmId) || rc.RootCauseDeviceId == id).ExecuteDeleteAsync();
            }
            
            await _context.Alarms.Where(a => a.DeviceId == id).ExecuteDeleteAsync();

            // 5. Finally, securely delete the device itself
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task AssignDeviceAsync(int deviceId, int userId, int? callerUserId = null)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null) throw new Exception("Device not found");

            if (callerUserId.HasValue)
            {
                var (role, layer) = await ResolveCallerAsync(callerUserId);
                if (!string.IsNullOrEmpty(role) && !role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue && device.DeviceType != layer.Value)
                        throw new UnauthorizedAccessException("Caller cannot assign this device");
                }
            }

            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists) throw new Exception("User not found");

            device.AssignedUserId = userId;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Device>> GetVisibleDevicesAsync(int userId)
        {
            var assignment = await _assignmentRepository.GetByUserId(userId);

            if (assignment == null)
                return new List<Device>();

            return assignment.AreaType switch
            {
                "LEA"      => await _deviceRepository.GetDevicesByLeaAsync(assignment.AreaId),
                "Province" => await _device_repository.GetDevicesByProvinceAsync(assignment.AreaId),
                "Region"   => await _device_repository.GetDevicesByRegionAsync(assignment.AreaId),
                _          => new List<Device>()
            };
        }

        public async Task<Device?> UpdateStatusAsync(int id, DeviceStatus status, int? callerUserId = null)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return null;

            if (callerUserId.HasValue)
            {
                var (role, layer) = await ResolveCallerAsync(callerUserId);
                if (!string.IsNullOrEmpty(role) && !role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue && device.DeviceType != layer.Value)
                        throw new UnauthorizedAccessException("Caller cannot update status of this device");
                }
            }

            var previous = device.Status;
            device.Status = status;
            await _context.SaveChangesAsync();

            // Clear alarms when device recovers to UP status
            if (previous != status && status == DeviceStatus.UP && 
                (previous == DeviceStatus.DOWN || previous == DeviceStatus.UNREACHABLE))
            {
                await _impactAnalysisService.ClearAlarmsAsync(id);
            }

            return device;
        }

        public async Task<Device?> SetSimulationStateAsync(int id, bool isSimulatedDown, int? callerUserId = null)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return null;

            if (callerUserId.HasValue)
            {
                var (role, layer) = await ResolveCallerAsync(callerUserId);
                if (!string.IsNullOrEmpty(role) && !role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && layer.HasValue && device.DeviceType != layer.Value)
                        throw new UnauthorizedAccessException("Caller cannot change simulation state for this device");
                }
            }

            // Simulation should only stop heartbeat emission. Do NOT force immediate Status change
            // or trigger propagation here. The background detector will observe missing heartbeats
            // and mark the device DOWN after the configured timeout.
            device.IsSimulatedDown = isSimulatedDown;

            await _context.SaveChangesAsync();
            return device;
        }
    }
}
