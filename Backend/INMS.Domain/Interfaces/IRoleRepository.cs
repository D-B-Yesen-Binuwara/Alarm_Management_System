using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role> AddAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task DeleteAsync(int id);
    }
}
