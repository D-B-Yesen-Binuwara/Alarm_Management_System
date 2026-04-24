using INMS.Application.DTOs;
using INMS.Domain.Entities;
using INMS.Domain.Enums;

namespace INMS.Application.Interfaces
{
    /// <summary>
    /// Service for managing device lifecycle, retrieval, and basic state updates.
    /// Impact-related operations are delegated to IImpactAnalysisService.
    /// 
    /// Responsibility: Device CRUD operations and basic status management (SRP)
    /// </summary>
    public interface IDeviceService
    {
        /// <summary>
        /// Retrieves all devices in the system.
        /// </summary>
        Task<IEnumerable<Device>> GetAllAsync();

        /// <summary>
        /// Retrieves all devices with dashboard-specific information (joins with LEA, Province, Region, User).
        /// </summary>
        Task<IEnumerable<DeviceListDto>> GetAllForDashboardAsync();

        /// <summary>
        /// Retrieves all devices with map visualization data (coordinates and impact status).
        /// </summary>
        Task<IEnumerable<DeviceMapDto>> GetDevicesForMapAsync();

        /// <summary>
        /// Retrieves a single device by ID.
        /// </summary>
        Task<Device?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new device.
        /// </summary>
        Task<Device> CreateAsync(CreateDeviceDto dto);

        /// <summary>
        /// Updates an existing device's properties.
        /// </summary>
        Task<Device?> UpdateAsync(int id, UpdateDeviceDto dto);

        /// <summary>
        /// Deletes a device and all associated records (links, alarms, heartbeats, etc.).
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Assigns a device to a user.
        /// </summary>
        Task AssignDeviceAsync(int deviceId, int userId);

        /// <summary>
        /// Retrieves devices visible to a user based on their area assignment.
        /// </summary>
        Task<List<Device>> GetVisibleDevicesAsync(int userId);

        /// <summary>
        /// Updates a device's status (delegating impact analysis to IImpactAnalysisService).
        /// </summary>
        Task<Device?> UpdateStatusAsync(int id, DeviceStatus status);

        /// <summary>
        /// Sets the simulation state for a device (forces DOWN for testing).
        /// </summary>
        Task<Device?> SetSimulationStateAsync(int id, bool isSimulatedDown);
    }
}
