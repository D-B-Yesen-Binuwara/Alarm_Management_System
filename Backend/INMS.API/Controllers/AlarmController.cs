using Microsoft.AspNetCore.Mvc;
using INMS.Application.Interfaces;

namespace INMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlarmController : ControllerBase
    {
        private readonly IImpactService _impactService;

        public AlarmController(IImpactService impactService)
        {
            _impactService = impactService;
        }

        [HttpGet("impact/{deviceId}")]
        public async Task<IActionResult> GetImpact(int deviceId)
        {
            var impacts = await _impactService.GetImpactedDevices(deviceId);

            return Ok(impacts);
        }
    }
}