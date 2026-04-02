using Microsoft.AspNetCore.Mvc;
using INMS.Application.Interfaces;
using INMS.Domain.Entities;

namespace INMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceLinkController : ControllerBase
    {
        private readonly IDeviceLinkService _service;

        public DeviceLinkController(IDeviceLinkService service)
        {
            _service = service;
        }

        // Create a parent-child link between two devices
        [HttpPost]
        public async Task<IActionResult> CreateLink(CreateLinkRequest request)
        {
            var link = await _service.CreateLinkAsync(request.ParentDeviceId, request.ChildDeviceId);
            return Ok(link);
        }

        // Fetch all device links
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllLinksAsync());
        }

        // Delete a device link by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteLinkAsync(id);
            return Ok("Deleted");
        }
    }

    public class CreateLinkRequest
    {
        public int ParentDeviceId { get; set; }
        public int ChildDeviceId { get; set; }
    }
}
