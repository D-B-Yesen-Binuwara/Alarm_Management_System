using INMS.Domain.Entities;

namespace INMS.Application.Interfaces;

public interface ISimulationEventService
{
    Task<SimulationEvent> LogEventAsync(int deviceId, string eventType);
    Task<List<SimulationEvent>> GetDeviceEventsAsync(int deviceId);
    Task<List<SimulationEvent>> GetAllEventsAsync();
}
