using INMS.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace INMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    // Fetch all users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    // Fetch a single user by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        return Ok(await _service.GetById(id));
    }

    // Create a new user with a username, password, and role
    [HttpPost]
    public async Task<IActionResult> Create(string username, string password, int roleId)
    {
        await _service.Create(username, password, roleId);
        return Ok();
    }



    // Delete a user by ID
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);
        return Ok();
    }
}
