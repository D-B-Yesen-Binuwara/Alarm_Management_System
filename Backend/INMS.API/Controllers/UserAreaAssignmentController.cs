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

    // Assign a user to a specific area (LEA, Province, or Region)
    [HttpPost]
    public async Task<IActionResult> Assign(int userId, string areaType, int areaId)
    {
        await _service.AssignArea(userId, areaType, areaId);
        return Ok();
    }
}
