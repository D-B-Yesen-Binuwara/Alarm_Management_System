using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;

namespace INMS.Application.Services
{
    public class RegionService : IRegionService
    {
        private readonly IRegionRepository _regionRepository;

        public RegionService(IRegionRepository regionRepository)
        {
            _regionRepository = regionRepository;
        }

        public async Task<List<Region>> GetAllRegionsAsync()
        {
            return await _regionRepository.GetAllAsync();
        }

        public async Task<Region?> GetRegionByIdAsync(int id)
        {
            return await _regionRepository.GetByIdAsync(id);
        }

        public async Task<Region> CreateRegionAsync(Region region)
        {
            return await _regionRepository.AddAsync(region);
        }

        public async Task<Region> UpdateRegionAsync(int id, Region region)
        {
            var existing = await _regionRepository.GetByIdAsync(id);

            if (existing == null)
                throw new Exception("Region not found");

            existing.Name = region.Name;
            existing.Description = region.Description;

            return await _regionRepository.UpdateAsync(existing);
        }

        public async Task DeleteRegionAsync(int id)
        {
            await _regionRepository.DeleteAsync(id);
        }
    }
}
