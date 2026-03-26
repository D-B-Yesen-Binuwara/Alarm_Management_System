using INMS.Domain.Entities;

namespace INMS.Application.Interfaces
{
    public interface IProvinceService
    {
        Task<List<Province>> GetAllAsync();
        Task<Province?> GetByIdAsync(int id);
        Task<Province> CreateAsync(Province province);
        Task<Province> UpdateAsync(int id, Province province);
        Task DeleteAsync(int id);
    }
}