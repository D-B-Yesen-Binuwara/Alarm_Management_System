using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;

namespace INMS.Application.Services
{
    public class ProvinceService : IProvinceService
    {
        private readonly IProvinceRepository _provinceRepository;
        private readonly IRegionRepository _regionRepository;

        public ProvinceService(
            IProvinceRepository provinceRepository,
            IRegionRepository regionRepository)
        {
            _provinceRepository = provinceRepository;
            _regionRepository = regionRepository;
        }

        public async Task<List<Province>> GetAllAsync()
        {
            return await _provinceRepository.GetAllAsync();
        }

        public async Task<Province?> GetByIdAsync(int id)
        {
            return await _provinceRepository.GetByIdAsync(id);
        }

        public async Task<Province> CreateAsync(Province province)
        {
            var region = await _regionRepository.GetByIdAsync(province.RegionId);

            if (region == null)
                throw new Exception("Region does not exist");

            return await _provinceRepository.AddAsync(province);
        }

        public async Task<Province> UpdateAsync(int id, Province province)
        {
            var existing = await _provinceRepository.GetByIdAsync(id);

            if (existing == null)
                throw new Exception("Province not found");

            existing.Name = province.Name;
            existing.RegionId = province.RegionId;

            return await _provinceRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            await _provinceRepository.DeleteAsync(id);
        }
    }
}
