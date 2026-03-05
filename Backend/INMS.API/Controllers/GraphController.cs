using Microsoft.AspNetCore.Mvc;
using INMS.Infrastructure.Persistence;
using System.Linq;

namespace INMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GraphController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetGraph()
        {
            var devices = _context.Devices.ToList();        // ✅ FIXED
            var links = _context.DeviceLinks.ToList();      // ✅ FIXED

            return Ok(new
            {
                devices,
                links
            });
        }
    }
}