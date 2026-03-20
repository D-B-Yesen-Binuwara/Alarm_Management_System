using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace INMS.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<User>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<User> GetById(int id)
    {
        return await _repository.GetById(id);
    }

    public async Task Create(string username, string password, int roleId)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            RoleId = roleId
        };

        await _repository.Create(user);
    }

    public async Task Update(int id, string username, int roleId)
    {
        var user = await _repository.GetById(id);

        user.Username = username;
        user.RoleId = roleId;

        await _repository.Update(user);
    }

    public async Task Delete(int id)
    {
        await _repository.Delete(id);
    }

    private string HashPassword(string password)
    {
        using SHA256 sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}