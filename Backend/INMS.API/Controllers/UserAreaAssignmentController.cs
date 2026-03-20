using INMS.Application.DTOs;
using INMS.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace INMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAreaAssignmentController : ControllerBase
{
    private readonly UserAreaAssignmentService _service;

    public UserAreaAssignmentController(UserAreaAssignmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var assignment = await _service.GetByUserIdAsync(userId);
        return assignment == null ? NotFound() : Ok(assignment);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(AssignAreaDto dto)
    {
        await _service.AssignArea(dto);
        return Ok();
    }

    [HttpDelete("{assignmentId}")]
    public async Task<IActionResult> Delete(int assignmentId)
    {
        await _service.DeleteAsync(assignmentId);
        return NoContent();
    }
}