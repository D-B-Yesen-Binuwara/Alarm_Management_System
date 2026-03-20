using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces
{
    public interface IRegionRepository
    {
        Task<List<Region>> GetAllAsync();
        Task<Region?> GetByIdAsync(int id);
        Task<Region> AddAsync(Region region);
        Task<Region> UpdateAsync(Region region);
        Task DeleteAsync(int id);
    }
}