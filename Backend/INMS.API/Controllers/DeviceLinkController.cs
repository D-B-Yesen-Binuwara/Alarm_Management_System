using Microsoft.AspNetCore.Mvc;
using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using System.Linq;

namespace INMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceLinkController : ControllerBase
    {
        private readonly IDeviceLinkService _service;
        private readonly INMS.Application.Interfaces.IDeviceService _deviceService;
        private readonly INMS.Domain.Interfaces.IUserRepository _userRepository;

        public DeviceLinkController(IDeviceLinkService service, INMS.Application.Interfaces.IDeviceService deviceService, INMS.Domain.Interfaces.IUserRepository userRepository)
        {
            _service = service;
            _deviceService = deviceService;
            _userRepository = userRepository;
        }

        private int? GetCallerUserIdFromHeader()
        {
            var idHeader = HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(idHeader) || !int.TryParse(idHeader, out var userId))
            {
                return null;
            }

            return userId;
        }
        {
            var idHeader = HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(idHeader) || !int.TryParse(idHeader, out var userId))
            {
                return (string.Empty, null);
            }

            var user = await _userRepository.GetById(userId);
            if (user == null) return (string.Empty, null);

            return (user.Role?.RoleName ?? string.Empty, user.Layer);
        }

        // Create a parent-child link between two devices
        [HttpPost]
        public async Task<IActionResult> CreateLink(CreateLinkRequest request)
        {
            var callerId = GetCallerUserIdFromHeader();

            try
            {
                var parent = await _deviceService.GetByIdAsync(request.ParentDeviceId, callerId);
                var child = await _deviceService.GetByIdAsync(request.ChildDeviceId, callerId);
                if (parent == null || child == null) return NotFound();

                var link = await _service.CreateLinkAsync(request.ParentDeviceId, request.ChildDeviceId);
                return Ok(link);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
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
