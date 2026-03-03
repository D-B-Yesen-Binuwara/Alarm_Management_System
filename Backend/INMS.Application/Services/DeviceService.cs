using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Enums;
using INMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INMS.Application.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly AppDbContext _context;

        public DeviceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Device>> GetAllAsync()
        {
            return await _context.Devices.ToListAsync();
        }

        public async Task<Device?> GetByIdAsync(int id)
        {
            return await _context.Devices.FindAsync(id);
        }

        public async Task<Device> CreateAsync(Device device)
        {
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<Device?> UpdateAsync(int id, Device device)
        {
            var existing = await _context.Devices.FindAsync(id);
            if (existing == null) return null;

            existing.DeviceName = device.DeviceName;
            existing.DeviceType = device.DeviceType;
            existing.IP = device.IP;
            existing.Status = device.Status;
            existing.PriorityLevel = device.PriorityLevel;
            existing.LEAId = device.LEAId;
            existing.AssignedUserId = device.AssignedUserId;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return false;

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
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