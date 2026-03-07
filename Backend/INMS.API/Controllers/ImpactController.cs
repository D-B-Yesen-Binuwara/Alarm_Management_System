using Microsoft.AspNetCore.Mvc;
using INMS.Infrastructure.Services;

namespace INMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImpactController : ControllerBase
    {
        private readonly ImpactService _impactService;

        public ImpactController(ImpactService impactService)
        {
            _impactService = impactService;
        }

        [HttpGet("{deviceId}")]
        public IActionResult GetImpact(int deviceId)
        {
            var impacted = _impactService.GetDirectImpacts(deviceId);

            return Ok(impacted);
        }
    }
}