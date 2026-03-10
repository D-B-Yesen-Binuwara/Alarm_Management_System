using Microsoft.AspNetCore.Mvc;
using INMS.Application.Interfaces;

namespace INMS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HeartbeatController : ControllerBase
{
    private readonly IHeartbeatService _heartbeatService;

    public HeartbeatController(IHeartbeatService heartbeatService)
    {
        _heartbeatService = heartbeatService;
    }

    [HttpPost]
    public async Task<IActionResult> RecordHeartbeat([FromBody] HeartbeatRequest request)
    {
        var heartbeat = await _heartbeatService.RecordHeartbeatAsync(request.DeviceId, request.Status);
        return Ok(heartbeat);
    }

    [HttpGet("device/{deviceId}")]
    public async Task<IActionResult> GetDeviceHeartbeats(int deviceId)
    {
        var heartbeats = await _heartbeatService.GetDeviceHeartbeatsAsync(deviceId);
        return Ok(heartbeats);
    }

    [HttpGet("device/{deviceId}/latest")]
    public async Task<IActionResult> GetLatestHeartbeat(int deviceId)
    {
        var heartbeat = await _heartbeatService.GetLatestHeartbeatAsync(deviceId);
        if (heartbeat == null) return NotFound();
        return Ok(heartbeat);
    }
}

public class HeartbeatRequest
{
    public int DeviceId { get; set; }
    public string Status { get; set; }
}
