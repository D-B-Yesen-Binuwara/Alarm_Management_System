using INM_FCS.Domain.Entities;

namespace INM_FCS.Application.Interfaces
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetAllAsync();
        Task<Device> GetByIdAsync(int id);
        Task<Device> CreateAsync(Device device);
        Task<Device> UpdateAsync(int id, Device device);
        Task<bool> DeleteAsync(int id);
    }
}