using Microsoft.EntityFrameworkCore;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;

namespace INMS.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _context;

    public DeviceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> GetByIdAsync(int id)
    {
        return await _context.Devices.FindAsync(id);
    }

    public async Task<List<Device>> GetAllAsync()
    {
        return await _context.Devices.ToListAsync();
    }

    public async Task AddAsync(Device device)
    {
        await _context.Devices.AddAsync(device);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Device device)
    {
        _context.Devices.Update(device);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Device>> GetDevicesByLeaAsync(int leaId)
    {
        return await _context.Devices
            .Where(d => d.LEAId == leaId)
            .ToListAsync();
    }

    public async Task<List<Device>> GetDevicesByProvinceAsync(int provinceId)
    {
        return await _context.Devices
            .Join(_context.LEAs,
                d => d.LEAId,
                l => l.LEAId,
                (d, l) => new { Device = d, LEA = l })
            .Where(x => x.LEA.ProvinceId == provinceId)
            .Select(x => x.Device)
            .ToListAsync();
    }

    public async Task<List<Device>> GetDevicesByRegionAsync(int regionId)
    {
        return await _context.Devices
            .Join(_context.LEAs,
                d => d.LEAId,
                l => l.LEAId,
                (d, l) => new { Device = d, LEA = l })
            .Join(_context.Provinces,
                x => x.LEA.ProvinceId,
                p => p.ProvinceId,
                (x, p) => new { x.Device, Province = p })
            .Where(x => x.Province.RegionId == regionId)
            .Select(x => x.Device)
            .ToListAsync();
    }

    public async Task<List<Device>> GetDevicesByDeviceTypeAsync(INMS.Domain.Enums.DeviceType deviceType)
    {
        return await _context.Devices
            .Where(d => d.DeviceType == deviceType)
            .ToListAsync();
    }
}
