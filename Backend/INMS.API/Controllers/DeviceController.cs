using Microsoft.AspNetCore.Mvc;
using INMS.Application.Interfaces;
using INMS.Domain.Entities;

namespace INMS.API.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var device = await _deviceService.GetByIdAsync(id);
            if (device == null) return NotFound();
            return Ok(device);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Device device)
        {
            var created = await _deviceService.CreateAsync(device);
            return CreatedAtAction(nameof(GetById), new { id = created.DeviceId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Device device)
        {
            var updated = await _deviceService.UpdateAsync(id, device);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _deviceService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id}/assign")]
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