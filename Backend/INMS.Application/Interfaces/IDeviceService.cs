using INMS.Application.DTOs;
using INMS.Domain.Entities;
using INMS.Domain.Enums;

namespace INMS.Application.Interfaces
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetAllAsync();
        Task<IEnumerable<DeviceListDto>> GetAllForDashboardAsync();
        Task<IEnumerable<DeviceMapDto>> GetDevicesForMapAsync();
        Task<Device?> GetByIdAsync(int id);
        Task<Device> CreateAsync(CreateDeviceDto dto);
        Task<Device?> UpdateAsync(int id, UpdateDeviceDto dto);
        Task PropagateImpact(int rootDeviceId);
        Task EnsureUnreachableAlarmAsync(int deviceId);
        Task<bool> DeleteAsync(int id);
        Task AssignDeviceAsync(int deviceId, int userId);
        Task<List<Device>> GetVisibleDevicesAsync(int userId);
        Task<Device?> UpdateStatusAsync(int id, DeviceStatus status);
        Task<Device?> SetSimulationStateAsync(int id, bool isSimulatedDown);
    }
}
