using INMS.Application.DTOs;
using INMS.Application.Interfaces;
using INMS.Domain.Entities;
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

        public async Task<IEnumerable<Device>> GetAllAsync() => await _deviceRepository.GetAllAsync();

        public async Task<IEnumerable<DeviceMapDto>> GetDevicesForMapAsync()
        {
            return await _context.Devices
                .Select(d => new DeviceMapDto(
                    d.DeviceId,
                    d.DeviceName,
                    d.DeviceType.ToString(),
                    d.Latitude,
                    d.Longitude,
                    d.Status,
                    _context.ImpactedDevices.Any(id => id.DeviceId == d.DeviceId) ? 1 : 0
                ))
                .ToListAsync();
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
                Status = "UP"
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<Device?> UpdateAsync(int id, UpdateDeviceDto dto)
        {
            var existing = await _deviceRepository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.DeviceName = dto.DeviceName;
            existing.DeviceType = dto.DeviceType;
            existing.IP = dto.IP ?? string.Empty;
            existing.Status = dto.Status;
            existing.PriorityLevel = dto.PriorityLevel;
            existing.LEAId = dto.LEAId;
            existing.Latitude = dto.Latitude;
            existing.Longitude = dto.Longitude;

            await _deviceRepository.UpdateAsync(existing);
            return existing;
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
            var device = await _deviceRepository.GetByIdAsync(deviceId)
                ?? throw new Exception("Device not found");

            var user = await _context.Users.FindAsync(userId)
                ?? throw new Exception("User not found");

            device.AssignedUserId = userId;
            await _deviceRepository.UpdateAsync(device);
        }

        public async Task<List<Device>> GetVisibleDevicesAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new Exception("User not found.");

            if (user.Role?.RoleName == "Admin")
                return await _deviceRepository.GetAllAsync();

            var assignment = await _assignmentRepository.GetByUserId(userId);
            if (assignment == null) return new List<Device>();

            return assignment.AreaType switch
            {
                "LEA"      => await _deviceRepository.GetDevicesByLeaAsync(assignment.AreaId),
                "Province" => await _deviceRepository.GetDevicesByProvinceAsync(assignment.AreaId),
                "Region"   => await _deviceRepository.GetDevicesByRegionAsync(assignment.AreaId),
                _          => new List<Device>()
            };
        }
    }
}
