using Microsoft.AspNetCore.Mvc;
using INMS.Application.Services;
using INMS.Application.Models;

namespace INMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CorrelationController : ControllerBase
    {
        private readonly CorrelationService _correlationService;

        // Constructor
        public CorrelationController(CorrelationService correlationService)
        {
            _correlationService = correlationService;
        }

        // -------------------------------------------------
        // GET: api/correlation/{deviceId}
        // Runs correlation and finds the root cause device
        // -------------------------------------------------
        [HttpGet("{deviceId}")]
        public IActionResult RunCorrelation(int deviceId)
        {
            try
            {
                CorrelationResult result = _correlationService.FindRootCause(deviceId);

                return Ok(new
                {
                    alarmDevice = deviceId,
                    rootCause = result.RootCauseDevice,
                    path = result.Path
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Correlation failed",
                    error = ex.Message
                });
            }
        }
    }
}