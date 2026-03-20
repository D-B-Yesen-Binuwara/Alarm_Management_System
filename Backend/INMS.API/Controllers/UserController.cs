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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        return Ok(await _service.GetById(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create(string username, string password, int roleId)
    {
        await _service.Create(username, password, roleId);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, string username, int roleId)
    {
        await _service.Update(id, username, roleId);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);
        return Ok();
    }
}