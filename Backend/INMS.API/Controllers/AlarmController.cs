using Microsoft.AspNetCore.Mvc;
using INMS.Infrastructure.Services;

namespace INMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlarmController : ControllerBase
    {
        private readonly CorrelationService _correlationService;
        private readonly ImpactService _impactService;

        public AlarmController(
            CorrelationService correlationService,
            ImpactService impactService)
        {
            _correlationService = correlationService;
            _impactService = impactService;
        }

        // Example: api/alarm/5
        [HttpGet("{deviceId}")]
        public IActionResult ProcessAlarm(int deviceId)
        {
            // Step 1: Find root cause
            var rootCauseId = _correlationService.FindRootCause(deviceId);

            // Step 2: Find impacted devices
            var impactedDevices = _impactService.GetDirectImpacts(rootCauseId);

            return Ok(new
            {
                AlarmDevice = deviceId,
                RootCauseDevice = rootCauseId,
                ImpactedDevices = impactedDevices
            });
        }
    }
}