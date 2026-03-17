using INMS.Domain.Entities;
using INMS.Domain.Enums;

namespace INMS.Application.Interfaces
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetAllAsync();
        Task<Device?> GetByIdAsync(int id);
        Task<Device> CreateAsync(Device device);
        Task<Device?> UpdateAsync(int id, Device device);
        Task<bool> DeleteAsync(int id);
        Task AssignDeviceAsync(int deviceId, int userId);
        Task<List<Device>> GetVisibleDevicesAsync(int userId);
        Task<Device?> UpdateStatusAsync(int id, DeviceStatus status);
    }
}
