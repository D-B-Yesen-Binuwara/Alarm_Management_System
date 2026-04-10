using INMS.Domain.Entities;
namespace INMS.Domain.Interfaces;

public interface IDeviceLinkRepository
{
    Task<DeviceLink> AddAsync(DeviceLink link);
    Task<List<DeviceLink>> GetAllAsync();
    Task DeleteAsync(int id);
}