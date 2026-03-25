using Microsoft.AspNetCore.Mvc;
using INMS.Application.Services;
using INMS.Infrastructure.Persistence;
using System.Linq;

namespace INMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CorrelationController : ControllerBase
    {
        private readonly CorrelationService _correlationService;
        private readonly AppDbContext _context;

        public CorrelationController(
            CorrelationService correlationService,
            AppDbContext context)
        {
            _correlationService = correlationService;
            _context = context;
        }

        [HttpGet("{deviceId}")]
        public IActionResult GetRootCause(int deviceId)
        {
            var rootId = _correlationService.FindRootCause(deviceId);

            var alarmDevice = _context.Devices
                .FirstOrDefault(x => x.DeviceId == deviceId);

            var rootDevice = _context.Devices
                .FirstOrDefault(x => x.DeviceId == rootId);

            if (alarmDevice == null || rootDevice == null)
                return NotFound();

            var path = new List<string>
            {
                alarmDevice.DeviceName,
                rootDevice.DeviceName
            };

            return Ok(new
            {
                alarmDevice = alarmDevice.DeviceName,
                rootCauseDevice = rootDevice.DeviceName,
                path = path,
                status = "DOWN",
                severity = "CRITICAL"
            });
        }
    }
}