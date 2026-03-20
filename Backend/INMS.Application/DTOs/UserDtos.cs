namespace INMS.Application.DTOs;

public record CreateUserDto(string Username, string Password, string FullName, int RoleId);
public record UpdateUserDto(string Username, string FullName, int RoleId);
public record UserResponseDto(int UserId, string Username, string FullName, int RoleId, string? RoleName);
