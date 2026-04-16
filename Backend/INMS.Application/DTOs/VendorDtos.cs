using System.ComponentModel.DataAnnotations;
using INMS.Domain.Enums;

namespace INMS.Application.DTOs;

public record CreateVendorDto(
    string Name,
    string Brand,
    DeviceType DeviceType,
    string? Description
);

public record UpdateVendorDto(
    string Name,
    string Brand,
    DeviceType DeviceType,
    string? Description,
    bool IsActive
);

public record VendorDto(
    int VendorId,
    string Name,
    string Brand,
    DeviceType DeviceType,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    int DeviceCount
);