using INMS.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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

    // 🔹 Fetch all users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    // ✅ Fetch a single user by ID - Returns proper JSON
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _service.GetById(id);
        if (user == null)
            return NotFound(new { error = "User not found" });

        return Ok(new
        {
            userId = user.UserId,
            username = user.Username,
            fullName = user.FullName,
            role = user.Role
        });
    }

    // 🔹 Create a new user
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Username and password are required" });

        await _service.Create(request.Username, request.Password, request.RoleId);
        return Ok(new { message = "User created successfully" });
    }

    // ✅ Update user - Fixed to accept proper DTO and return updated user
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new { error = "Username is required" });

            await _service.Update(id, request.Username, request.RoleId);

            // ✅ Return updated user data so frontend can sync
            var user = await _service.GetById(id);
            if (user == null)
                return NotFound(new { error = "User not found" });

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                fullName = user.FullName,
                role = user.Role,
                message = "Profile updated successfully"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ✅ Update user password
    [HttpPut("{id}/password")]
    public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordRequest request)
    {
        try
        {
            var user = await _service.GetById(id);
            if (user == null)
                return NotFound(new { error = "User not found" });

            // Verify old password
            if (user.PasswordHash != request.OldPassword)
                return Unauthorized(new { error = "Invalid current password" });

            // Update password (in production, use proper hashing like BCrypt)
            user.PasswordHash = request.NewPassword;
            await _service.Update(id, user.Username, user.Role?.RoleId ?? 1);

            return Ok(new { message = "Password updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // 🔹 Delete user
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.Delete(id);
            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // 🔥🔥🔥 LOGIN API - Validates from database (NOT hardcoded)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Username and password are required" });

            var users = await _service.GetAll();

            // ✅ Filter by Username (not Email) - Database validation
            var user = users.FirstOrDefault(u =>
                u.Username == request.Username &&
                u.PasswordHash == request.Password);

            if (user == null)
                return Unauthorized(new { error = "Invalid username or password" });

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                fullName = user.FullName,
                role = user.Role
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// ✅ DTOs for proper request/response handling
public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public int RoleId { get; set; }
}

public class UpdateUserRequest
{
    public string Username { get; set; }
    public int RoleId { get; set; }
}

public class UpdatePasswordRequest
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}