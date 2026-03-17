using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Domain.Enums;
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

        public async Task<Device?> GetByIdAsync(int id)
        {
            return await _deviceRepository.GetByIdAsync(id);
        }

        public async Task<Device> CreateAsync(Device device)
        {
            if (device.AssignedUserId.HasValue)
            {
                var userExists = await _context.Users
                    .AnyAsync(u => u.UserId == device.AssignedUserId.Value);

                if (!userExists)
                    throw new Exception("Assigned user does not exist.");
            }

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<Device?> UpdateAsync(int id, Device device)
        {
            var existing = await _deviceRepository.GetByIdAsync(id);
            if (existing == null) return null;

            if (device.AssignedUserId.HasValue)
            {
                var userExists = await _context.Users
                    .AnyAsync(u => u.UserId == device.AssignedUserId.Value);

                if (!userExists)
                    throw new Exception("Assigned user does not exist.");
            }

            existing.DeviceName = device.DeviceName;
            existing.DeviceType = device.DeviceType;
            existing.IP = device.IP;
            existing.Status = device.Status;
            existing.PriorityLevel = device.PriorityLevel;
            existing.LEAId = device.LEAId;
            existing.AssignedUserId = device.AssignedUserId;

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
