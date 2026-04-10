using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace INMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegionController : ControllerBase
    {
        private readonly IRegionService _regionService;

        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        // Fetch all regions
        [HttpGet]
        public async Task<IActionResult> GetAllRegions()
        {
            var regions = await _regionService.GetAllRegionsAsync();
            return Ok(regions);
        }

        // Fetch a single region by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegion(int id)
        {
            var region = await _regionService.GetRegionByIdAsync(id);
            if (region == null) return NotFound();
            return Ok(region);
        }

        // Create a new region
        [HttpPost]
        public async Task<IActionResult> CreateRegion([FromBody] Region region)
        {
            var created = await _regionService.CreateRegionAsync(region);
            return Ok(created);
        }

        // Update an existing region by ID
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegion(int id, [FromBody] Region region)
        {
            var updated = await _regionService.UpdateRegionAsync(id, region);
            return Ok(updated);
        }

        // Delete a region by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegion(int id)
        {
            await _regionService.DeleteRegionAsync(id);
            return Ok("Region deleted successfully");
        }
    }
}
