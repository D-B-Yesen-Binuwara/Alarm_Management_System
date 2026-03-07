using Microsoft.AspNetCore.Mvc;
using INMS.Application.Services;

namespace INMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CorrelationController : ControllerBase
    {
        private readonly CorrelationService _correlationService;

        public CorrelationController(CorrelationService correlationService)
        {
            _correlationService = correlationService;
        }

        [HttpGet("{deviceId}")]
        public IActionResult GetRootCause(int deviceId)
        {
            var root = _correlationService.FindRootCause(deviceId);

            return Ok(new
            {
                AlarmDevice = deviceId,
                RootCauseDevice = root
            });
        }
    }
}