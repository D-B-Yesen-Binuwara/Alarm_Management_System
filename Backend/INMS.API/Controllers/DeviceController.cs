using Microsoft.AspNetCore.Mvc;
using INMS.Application.Services;

namespace INMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceController : ControllerBase
{
    private readonly DeviceService _deviceService;

    public DeviceController(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var devices = await _deviceService.GetAllDevices();
        return Ok(devices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var device = await _deviceService.GetDeviceById(id);
        if (device == null)
            return NotFound();
        return Ok(device);
    }
}
