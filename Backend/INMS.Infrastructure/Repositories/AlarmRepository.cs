using Microsoft.EntityFrameworkCore;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;

namespace INMS.Infrastructure.Repositories;

public class AlarmRepository : IAlarmRepository
{
    private readonly AppDbContext _context;

    public AlarmRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Alarm> GetByIdAsync(int id)
    {
        return await _context.Alarms.FindAsync(id);
    }

    public async Task<List<Alarm>> GetAllAsync()
    {
        return await _context.Alarms.ToListAsync();
    }

    public async Task<List<Alarm>> GetByDeviceIdAsync(int deviceId)
    {
        return await _context.Alarms
            .Where(a => a.DeviceId == deviceId)
            .ToListAsync();
    }

    public async Task<Alarm> AddAsync(Alarm alarm)
    {
        await _context.Alarms.AddAsync(alarm);
        await _context.SaveChangesAsync();
        return alarm;
    }

    public async Task<Alarm> UpdateAsync(Alarm alarm)
    {
        _context.Alarms.Update(alarm);
        await _context.SaveChangesAsync();
        return alarm;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var alarm = await _context.Alarms.FindAsync(id);
        if (alarm == null) return false;

        _context.Alarms.Remove(alarm);
        await _context.SaveChangesAsync();
        return true;
    }
}
