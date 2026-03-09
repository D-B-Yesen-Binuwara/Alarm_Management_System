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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Province province)
        {
            return Ok(await _service.CreateAsync(province));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Province province)
        {
            return Ok(await _service.UpdateAsync(id, province));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok("Deleted");
        }
    }
}
