using INMS.Application.DTOs;
using INMS.Domain.Entities;

namespace INMS.Application.Interfaces
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetAllAsync();
        Task<Device?> GetByIdAsync(int id);
        Task<Device> CreateAsync(CreateDeviceDto dto);
        Task<Device?> UpdateAsync(int id, UpdateDeviceDto dto);
        Task<bool> DeleteAsync(int id);
        Task AssignDeviceAsync(int deviceId, int userId);
        Task<List<Device>> GetVisibleDevicesAsync(int userId);
    }
}
