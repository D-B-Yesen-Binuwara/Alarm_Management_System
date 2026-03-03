using Microsoft.EntityFrameworkCore;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;

namespace INMS.Infrastructure.Repositories;

public class HeartbeatRepository : IHeartbeatRepository
{
    private readonly AppDbContext _context;

    public HeartbeatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Heartbeat> AddAsync(Heartbeat heartbeat)
    {
        await _context.Heartbeats.AddAsync(heartbeat);
        await _context.SaveChangesAsync();
        return heartbeat;
    }

    public async Task<List<Heartbeat>> GetByDeviceIdAsync(int deviceId)
    {
        return await _context.Heartbeats
            .Where(h => h.DeviceId == deviceId)
            .OrderByDescending(h => h.Timestamp)
            .ToListAsync();
    }

    public async Task<Heartbeat> GetLatestByDeviceIdAsync(int deviceId)
    {
        return await _context.Heartbeats
            .Where(h => h.DeviceId == deviceId)
            .OrderByDescending(h => h.Timestamp)
            .FirstOrDefaultAsync();
    }
}
