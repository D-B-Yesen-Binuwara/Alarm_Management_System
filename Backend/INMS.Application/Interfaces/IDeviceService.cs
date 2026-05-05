using INMS.Application.DTOs;
using INMS.Domain.Entities;
using INMS.Domain.Enums;

namespace INMS.Application.Interfaces
{
    // Service for managing device lifecycle, retrieval, and basic state updates.
    // Impact-related operations are delegated to IImpactAnalysisService.
    // Responsibility: Device CRUD operations and basic status management (SRP)
    public interface IDeviceService
    {
        // Retrieves all devices in the system.
        Task<IEnumerable<Device>> GetAllAsync(int? callerUserId = null);

        // Retrieves all devices with dashboard-specific information (joins with LEA, Province, Region, User).
        Task<IEnumerable<DeviceListDto>> GetAllForDashboardAsync(int? callerUserId = null);

        // Retrieves all devices with map visualization data (coordinates and impact status).
        Task<IEnumerable<DeviceMapDto>> GetDevicesForMapAsync(int? callerUserId = null);

        // Retrieves a single device by ID. Optional callerUserId enforces access rules when provided.
        Task<Device?> GetByIdAsync(int id, int? callerUserId = null);

        // Creates a new device. Optional callerUserId enforces access rules when provided.
        Task<Device> CreateAsync(CreateDeviceDto dto, int? callerUserId = null);

        // Updates an existing device's properties. Optional callerUserId enforces access rules when provided.
        Task<Device?> UpdateAsync(int id, UpdateDeviceDto dto, int? callerUserId = null);

        // Deletes a device and all associated records (links, alarms, heartbeats, etc.). Optional callerUserId enforces access rules when provided.
        Task<bool> DeleteAsync(int id, int? callerUserId = null);

        // Assigns a device to a user. Optional callerUserId enforces access rules when provided.
        Task AssignDeviceAsync(int deviceId, int userId, int? callerUserId = null);

        // Retrieves devices visible to a user based on their area assignment.
        Task<List<Device>> GetVisibleDevicesAsync(int userId);

        // Updates a device's status (delegating impact analysis to IImpactAnalysisService). Optional callerUserId enforces access rules when provided.
        Task<Device?> UpdateStatusAsync(int id, DeviceStatus status, int? callerUserId = null);

        // Sets the simulation state for a device (forces DOWN for testing). Optional callerUserId enforces access rules when provided.
        Task<Device?> SetSimulationStateAsync(int id, bool isSimulatedDown, int? callerUserId = null);
    }
}
