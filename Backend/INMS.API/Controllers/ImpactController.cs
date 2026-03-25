using INMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace INMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImpactController : ControllerBase
    {
        private readonly IImpactService _impactService;

        public ImpactController(IImpactService impactService)
        {
            _impactService = impactService;
        }

        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetImpact(int deviceId)
        {
            var result = await _impactService.GetImpactedDevices(deviceId);

            return Ok(result);
        }
    }
}