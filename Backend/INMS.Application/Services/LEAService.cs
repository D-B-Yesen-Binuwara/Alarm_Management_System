using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;

namespace INMS.Application.Services
{
    public class LEAService : ILEAService
    {
        private readonly ILEARepository _leaRepository;
        private readonly IProvinceRepository _provinceRepository;

        public LEAService(ILEARepository leaRepository, IProvinceRepository provinceRepository)
        {
            _leaRepository = leaRepository;
            _provinceRepository = provinceRepository;
        }

        public async Task<List<LEA>> GetAllAsync()
        {
            return await _leaRepository.GetAllAsync();
        }

        public async Task<LEA?> GetByIdAsync(int id)
        {
            return await _leaRepository.GetByIdAsync(id);
        }

        public async Task<LEA> CreateAsync(LEA lea)
        {
            var province = await _provinceRepository.GetByIdAsync(lea.ProvinceId);

            if (province == null)
                throw new Exception("Province does not exist");

            return await _leaRepository.AddAsync(lea);
        }

        public async Task<LEA> UpdateAsync(int id, LEA lea)
        {
            var existing = await _leaRepository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("LEA not found");

            var province = await _provinceRepository.GetByIdAsync(lea.ProvinceId);
            if (province == null)
                throw new Exception("Province does not exist");

            existing.Name = lea.Name;
            existing.ProvinceId = lea.ProvinceId;

            return await _leaRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            await _leaRepository.DeleteAsync(id);
        }
    }
}
