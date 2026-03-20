using INMS.Domain.Enums;

namespace INMS.Application.DTOs;

public record CreateDeviceDto(
	string DeviceName,
	DeviceType DeviceType,
	string? IP,
	PriorityLevel PriorityLevel,
	int LEAId,
	decimal? Latitude,
	decimal? Longitude
);

public record UpdateDeviceDto(
	string DeviceName,
	DeviceType DeviceType,
	string? IP,
	string Status,
	PriorityLevel PriorityLevel,
	int LEAId,
	decimal? Latitude,
	decimal? Longitude
);

public record DeviceMapDto(
	int DeviceId,
	string DeviceName,
	string DeviceType,
	decimal? Latitude,
	decimal? Longitude,
	string Status,
	int IsImpacted
);
