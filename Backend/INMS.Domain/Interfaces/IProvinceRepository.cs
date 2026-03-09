using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces
{
    public interface IProvinceRepository
    {
        Task<List<Province>> GetAllAsync();
        Task<Province?> GetByIdAsync(int id);
        Task<Province> AddAsync(Province province);
        Task<Province> UpdateAsync(Province province);
        Task DeleteAsync(int id);
    }
}