using Microsoft.AspNetCore.Mvc;
using INMS.Application.Interfaces;

namespace INMS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SimulationEventController : ControllerBase
{
    private readonly ISimulationEventService _simulationEventService;

    public SimulationEventController(ISimulationEventService simulationEventService)
    {
        _simulationEventService = simulationEventService;
    }

    // Log a new simulation event for a device
    [HttpPost]
    public async Task<IActionResult> LogEvent([FromBody] SimulationEventRequest request)
    {
        var simulationEvent = await _simulationEventService.LogEventAsync(request.DeviceId, request.EventType);
        return Ok(simulationEvent);
    }

    // Fetch all simulation events
    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _simulationEventService.GetAllEventsAsync();
        return Ok(events);
    }

    // Fetch all simulation events for a specific device
    [HttpGet("device/{deviceId}")]
    public async Task<IActionResult> GetDeviceEvents(int deviceId)
    {
        var events = await _simulationEventService.GetDeviceEventsAsync(deviceId);
        return Ok(events);
    }
}

public class SimulationEventRequest
{
    public int DeviceId { get; set; }
    public string EventType { get; set; } = string.Empty;
}
