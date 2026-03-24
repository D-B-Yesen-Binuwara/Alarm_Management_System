using INMS.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using INMS.Application.Interfaces;

namespace INMS.API.Controllers
{
    [Route("api/device")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DeviceController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _deviceService.GetAllAsync());

        [HttpGet("visible/{userId}")]
        public async Task<IActionResult> GetVisible(int userId) => Ok(await _deviceService.GetVisibleDevicesAsync(userId));

        [HttpGet("map")]
        public async Task<IActionResult> GetMapData() => Ok(await _deviceService.GetDevicesForMapAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var device = await _deviceService.GetByIdAsync(id);
            return device == null ? NotFound() : Ok(device);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDeviceDto dto)
        {
            var created = await _deviceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.DeviceId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateDeviceDto dto)
        {
            var updated = await _deviceService.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _deviceService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPatch("{id:int}/assign")]
        public async Task<IActionResult> AssignDevice(int id, [FromBody] AssignDeviceRequest request)
        {
            await _deviceService.AssignDeviceAsync(id, request.UserId);
            return Ok("Device assigned successfully");
        }

        public class AssignDeviceRequest
        {
            public int UserId { get; set; }
        }
    }
}
