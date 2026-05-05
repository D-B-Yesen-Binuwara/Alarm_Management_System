namespace INMS.Application.DTOs;

using INMS.Domain.Enums;

public record CreateUserDto(
    string FirstName,
    string LastName,
    int RoleId,
    DeviceType? Layer = null,
    string? ServiceId = null,
    string? Email = null,
    int? RegionId = null,
    int? ProvinceId = null,
    int? LEAId = null
);

public record UpdateUserDto(string Username, string FullName, int RoleId, DeviceType? Layer = null, string? ServiceId = null, string? Email = null);

public record UserResponseDto(int UserId, string Username, string FullName, int RoleId, string? RoleName, DeviceType? Layer, string? ServiceId, string? Email, string? Region, string? Province, string? LEA);
