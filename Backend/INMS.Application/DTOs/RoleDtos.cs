namespace INMS.Application.DTOs;

public record CreateRoleDto(string RoleName, string? Description);
public record UpdateRoleDto(string RoleName, string? Description);
