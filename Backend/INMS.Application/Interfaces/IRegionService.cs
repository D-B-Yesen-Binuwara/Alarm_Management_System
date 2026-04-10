using INMS.Domain.Entities;

namespace INMS.Application.Interfaces
{
    public interface IRegionService
    {
        Task<List<Region>> GetAllRegionsAsync();
        Task<Region?> GetRegionByIdAsync(int id);
        Task<Region> CreateRegionAsync(Region region);
        Task<Region> UpdateRegionAsync(int id, Region region);
        Task DeleteRegionAsync(int id);
    }
}

