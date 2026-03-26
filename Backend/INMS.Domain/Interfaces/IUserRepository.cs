using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAll();
        Task<User?> GetById(int id);
        Task Create(User user);
        Task Update(User user);
        Task Delete(int id);
    }
}
