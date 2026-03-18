using INMS.Application.DTOs;
using INMS.Domain.Entities;

namespace INMS.Application.Interfaces
{
    public interface IRoleService
    {
        Task<List<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role> CreateAsync(CreateRoleDto dto);
        Task<Role> UpdateAsync(int id, UpdateRoleDto dto);
        Task DeleteAsync(int id);
    }
}
