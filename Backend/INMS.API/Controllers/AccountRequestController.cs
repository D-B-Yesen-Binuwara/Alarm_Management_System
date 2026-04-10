using INMS.Application.DTOs;
using INMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace INMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountRequestController : ControllerBase
{
    private readonly IAccountRequestService _service;

    public AccountRequestController(IAccountRequestService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] CreateAccountRequestDto dto)
    {
        await _service.Submit(dto);
        return Ok();
    }
}
