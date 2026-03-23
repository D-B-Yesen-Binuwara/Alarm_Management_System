using Microsoft.EntityFrameworkCore;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;

namespace INMS.Infrastructure.Repositories;

public class SimulationEventRepository : ISimulationEventRepository
{
    private readonly AppDbContext _context;

    public SimulationEventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SimulationEvent> AddAsync(SimulationEvent simulationEvent)
    {
        await _context.SimulationEvents.AddAsync(simulationEvent);
        await _context.SaveChangesAsync();
        return simulationEvent;
    }

    public async Task<List<SimulationEvent>> GetByDeviceIdAsync(int deviceId)
    {
        return await _context.SimulationEvents
            .Where(e => e.DeviceId == deviceId)
            .OrderByDescending(e => e.EventTime)
            .ToListAsync();
    }

    public async Task<List<SimulationEvent>> GetAllAsync()
    {
        return await _context.SimulationEvents
            .OrderByDescending(e => e.EventTime)
            .ToListAsync();
    }
}
