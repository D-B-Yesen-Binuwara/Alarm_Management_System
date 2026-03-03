using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Application.Interfaces;

namespace INMS.Application.Services;

public class SimulationEventService : ISimulationEventService
{
    private readonly ISimulationEventRepository _repository;

    public SimulationEventService(ISimulationEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<SimulationEvent> LogEventAsync(int deviceId, string eventType)
    {
        var simulationEvent = new SimulationEvent
        {
            DeviceId = deviceId,
            EventType = eventType,
            EventTime = DateTime.Now
        };

        return await _repository.AddAsync(simulationEvent);
    }

    public async Task<List<SimulationEvent>> GetDeviceEventsAsync(int deviceId)
    {
        return await _repository.GetByDeviceIdAsync(deviceId);
    }

    public async Task<List<SimulationEvent>> GetAllEventsAsync()
    {
        return await _repository.GetAllAsync();
    }
}
