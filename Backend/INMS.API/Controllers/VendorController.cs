using Microsoft.AspNetCore.Mvc;
using INMS.Application.Interfaces;
using INMS.Application.DTOs;
using INMS.Domain.Enums;

namespace INMS.API.Controllers
{
    [Route("api/vendor")]
    [ApiController]
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _vendorService;

        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        // Get all vendors
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vendors = await _vendorService.GetAllAsync();
            return Ok(vendors);
        }

        // Get vendor by ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vendor = await _vendorService.GetByIdAsync(id);
            if (vendor == null) return NotFound();
            return Ok(vendor);
        }

        // Get vendors by device type
        [HttpGet("device-type/{deviceType}")]
        public async Task<IActionResult> GetByDeviceType(DeviceType deviceType)
        {
            var vendors = await _vendorService.GetByDeviceTypeAsync(deviceType);
            return Ok(vendors);
        }

        // Get vendors by brand
        [HttpGet("brand/{brand}")]
        public async Task<IActionResult> GetByBrand(string brand)
        {
            var vendors = await _vendorService.GetByBrandAsync(brand);
            return Ok(vendors);
        }

        // Create new vendor
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVendorDto dto)
        {
            var created = await _vendorService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.VendorId }, created);
        }

        // Update vendor
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVendorDto dto)
        {
            var updated = await _vendorService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // Delete vendor
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _vendorService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}