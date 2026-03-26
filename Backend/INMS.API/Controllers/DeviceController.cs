using Microsoft.AspNetCore.Mvc;
using INMS.Application.DTOs;
using INMS.Application.Interfaces;
using INMS.Domain.Enums;

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
        public async Task<IActionResult> GetAll()
        {
            var devices = await _deviceService.GetAllAsync();
            return Ok(devices);
        }

        [HttpGet("visible/{userId}")]
        public async Task<IActionResult> GetVisible(int userId)
        {
            var devices = await _deviceService.GetVisibleDevicesAsync(userId);
            return Ok(devices);
        }

        [HttpGet("map")]
        public async Task<IActionResult> GetMapData() => Ok(await _deviceService.GetDevicesForMapAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var device = await _deviceService.GetByIdAsync(id);
            if (device == null) return NotFound();
            return Ok(device);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeviceDto dto)
        {
            var created = await _deviceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.DeviceId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDeviceDto dto)
        {
            var updated = await _deviceService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _deviceService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id:int}/assign")]
        public async Task<IActionResult> AssignDevice(int id, [FromBody] AssignDeviceRequest request)
        {
            await _deviceService.AssignDeviceAsync(id, request.UserId);
            return Ok("Device assigned successfully");
        }

        [HttpPost("{id}/simulate-failure")]
        public async Task<IActionResult> SimulateFailure(int id)
        {
            var updated = await _deviceService.SetSimulationStateAsync(id, true);
            if (updated == null) return NotFound();

            return Ok(new { message = "Device failure simulation started", deviceId = id });
        }

        [HttpPost("{id}/recover")]
        public async Task<IActionResult> Recover(int id)
        {
            var updated = await _deviceService.SetSimulationStateAsync(id, false);
            if (updated == null) return NotFound();

            return Ok(new { message = "Device recovery simulation started", deviceId = id });
        }

        public class AssignDeviceRequest
        {
            public int UserId { get; set; }
        }
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var updated = await _deviceService.UpdateStatusAsync(id, request.Status);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
    }

    public class UpdateStatusRequest
    {
        public DeviceStatus Status { get; set; }
    }
}
