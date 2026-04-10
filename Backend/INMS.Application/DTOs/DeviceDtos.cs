using System.ComponentModel.DataAnnotations;
using INMS.Domain.Enums;

namespace INMS.Application.DTOs;

public record CreateDeviceDto(
	string DeviceName,
	DeviceType DeviceType,
	string? IP,
	PriorityLevel PriorityLevel,
	int LEAId,
	[Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
	decimal Latitude,
	[Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
	decimal Longitude
);

public record UpdateDeviceDto(
	string DeviceName,
	DeviceType DeviceType,
	string? IP,
	string Status,
	PriorityLevel PriorityLevel,
	int LEAId,
	[Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
	decimal Latitude,
	[Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
	decimal Longitude
);

public record DeviceMapDto(
	int DeviceId,
	string DeviceName,
	string DeviceType,
	decimal Latitude,
	decimal Longitude,
	string Status,
	int IsImpacted
);

public record DeviceListDto(
	int DeviceId,
	string DeviceName,
	DeviceType DeviceType,
	string IP,
	DeviceStatus Status,
	PriorityLevel PriorityLevel,
	int LEAId,
	string? LEAName,
	string? ProvinceName,
	string? RegionName,
	decimal Latitude,
	decimal Longitude,
	int? AssignedUserId,
	string? AssignedUserFullName,
	string? AssignedUserServiceId,
	bool IsSimulatedDown
);
