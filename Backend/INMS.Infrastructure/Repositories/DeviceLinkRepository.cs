using Microsoft.EntityFrameworkCore;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;

namespace INMS.Infrastructure.Repositories;

public class DeviceLinkRepository : IDeviceLinkRepository
{
    private readonly AppDbContext _context;

    public DeviceLinkRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceLink> AddAsync(DeviceLink link)
    {
        await _context.DeviceLinks.AddAsync(link);
        await _context.SaveChangesAsync();
        return link;
    }

    public async Task<List<DeviceLink>> GetAllAsync()
    {
        return await _context.DeviceLinks.ToListAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var link = await _context.DeviceLinks.FindAsync(id);
        if (link == null)
            throw new Exception("Link not found");

        _context.DeviceLinks.Remove(link);
        await _context.SaveChangesAsync();
    }
}