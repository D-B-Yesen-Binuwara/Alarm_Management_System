using Microsoft.AspNetCore.Mvc;
using System.Linq;
using INMS.Infrastructure.Persistence;

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
            var devices = _context.Devices.ToList();
            var links = _context.DeviceLinks.ToList();

            var nodes = devices.Select(d => new
            {
                id = d.DeviceId,
                label = d.DeviceName
            });

            var edges = links.Select(l => new
            {
                from = l.ParentDeviceId,
                to = l.ChildDeviceId
            });

            return Ok(new { nodes, edges });
        }
    }
}