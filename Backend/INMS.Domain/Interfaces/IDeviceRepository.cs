using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces;

public interface IDeviceRepository
{
    Task<Device> GetByIdAsync(int id);
    Task<List<Device>> GetAllAsync();
    Task AddAsync(Device device);
    Task UpdateAsync(Device device);
}
