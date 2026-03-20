using INMS.Domain.Enums;

namespace INMS.Application.DTOs;

public record CreateDeviceDto(string DeviceName, DeviceType DeviceType, string? IP, PriorityLevel PriorityLevel, int LEAId);
public record UpdateDeviceDto(string DeviceName, DeviceType DeviceType, string? IP, string Status, PriorityLevel PriorityLevel, int LEAId);
