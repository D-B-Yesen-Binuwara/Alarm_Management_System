using INMS.Domain.Entities;

namespace INMS.Application.Interfaces;

public interface IDeviceLinkService
{
    Task<DeviceLink> CreateLinkAsync(int parentId, int childId);
    Task<List<DeviceLink>> GetAllLinksAsync();
    Task DeleteLinkAsync(int id);
}