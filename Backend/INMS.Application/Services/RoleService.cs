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

        public async Task<List<Role>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Role> CreateAsync(Role role)
        {
            return await _repository.AddAsync(role);
        }

        public async Task<Role> UpdateAsync(int id, Role role)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Role not found");

            existing.RoleName = role.RoleName;
            existing.Description = role.Description;

            return await _repository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
