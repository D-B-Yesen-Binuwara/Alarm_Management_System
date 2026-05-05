using INMS.Domain.Entities;
using INMS.Domain.Enums;

namespace INMS.Domain.Interfaces;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(int id);
    Task<List<Device>> GetAllAsync();
    Task AddAsync(Device device);
    Task UpdateAsync(Device device);
    Task<List<Device>> GetDevicesByLeaAsync(int leaId);
    Task<List<Device>> GetDevicesByProvinceAsync(int provinceId);
    Task<List<Device>> GetDevicesByRegionAsync(int regionId);
    Task<List<Device>> GetDevicesByDeviceTypeAsync(DeviceType deviceType);
}
