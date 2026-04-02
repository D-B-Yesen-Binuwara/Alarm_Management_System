using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace INMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvinceController : ControllerBase
    {
        private readonly IProvinceService _service;

        public ProvinceController(IProvinceService service)
        {
            _service = service;
        }

        // Fetch all provinces
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // Create a new province
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Province province)
        {
            return Ok(await _service.CreateAsync(province));
        }

        // Update an existing province by ID
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Province province)
        {
            return Ok(await _service.UpdateAsync(id, province));
        }

        // Delete a province by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok("Deleted");
        }
    }
}
