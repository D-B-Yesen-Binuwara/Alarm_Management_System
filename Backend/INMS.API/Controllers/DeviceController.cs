using Microsoft.AspNetCore.Mvc;
using INMS.Infrastructure.Persistence;
using System.Linq;

namespace INMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeviceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetDevices()
        {
            var devices = _context.Devices
                .Select(d => new
                {
                    id = d.DeviceId,
                    name = d.DeviceName
                })
                .ToList();

            return Ok(devices);
        }
    }
}