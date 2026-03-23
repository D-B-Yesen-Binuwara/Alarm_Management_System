using INMS.Domain.Entities;

namespace INMS.Application.Services;

public interface IUserService
{
    Task<List<User>> GetAll();
    Task<User> GetById(int id);
    Task Create(string username, string password, int roleId);
    Task Update(int id, string username, int roleId);
    Task Delete(int id);
}