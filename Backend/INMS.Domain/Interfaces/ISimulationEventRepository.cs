using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces;

public interface ISimulationEventRepository
{
    Task<SimulationEvent> AddAsync(SimulationEvent simulationEvent);
    Task<List<SimulationEvent>> GetByDeviceIdAsync(int deviceId);
    Task<List<SimulationEvent>> GetAllAsync();
}
