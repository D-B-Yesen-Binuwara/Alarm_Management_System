using Microsoft.AspNetCore.Mvc;
using INMS.Application.Services;
using INMS.Infrastructure.Services;

namespace INMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpGet("{deviceId}")]
        public IActionResult ProcessAlarm(int deviceId)
        {
            var rootCause = _correlationService.FindRootCause(deviceId);

            var impactedDevices = _impactService.GetDirectImpacts(rootCause);

            return Ok(new
            {
                AlarmDevice = deviceId,
                RootCauseDevice = rootCause,
                ImpactedDevices = impactedDevices
            });
        }
    }
}