using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace INMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // Fetch all roles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _roleService.GetAllAsync());
        }

        // Fetch a single role by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        // Create a new role
        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            var created = await _roleService.CreateAsync(role);
            return CreatedAtAction(nameof(GetById), new { id = created.RoleId }, created);
        }

        // Update an existing role by ID
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Role role)
        {
            var updated = await _roleService.UpdateAsync(id, role);
            return Ok(updated);
        }

        // Delete a role by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _roleService.DeleteAsync(id);
            return NoContent();
        }
    }
}
