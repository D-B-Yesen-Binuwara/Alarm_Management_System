using INMS.Application.DTOs;
using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;

namespace INMS.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _repository;

        public RoleService(IRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Role>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Role?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<Role> CreateAsync(CreateRoleDto dto)
        {
            var role = new Role { RoleName = dto.RoleName, Description = dto.Description };
            return await _repository.AddAsync(role);
        }

        public async Task<Role> UpdateAsync(int id, UpdateRoleDto dto)
        {
            var existing = await _repository.GetByIdAsync(id)
                ?? throw new Exception("Role not found");

            existing.RoleName = dto.RoleName;
            existing.Description = dto.Description;

            return await _repository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id) => await _repository.DeleteAsync(id);
    }
}
