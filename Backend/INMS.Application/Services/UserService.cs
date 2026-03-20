using INMS.Application.DTOs;
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

    public async Task<List<UserResponseDto>> GetAll()
    {
        var users = await _repository.GetAll();
        return users.Select(ToDto).ToList();
    }

    public async Task<UserResponseDto?> GetById(int id)
    {
        var user = await _repository.GetById(id);
        return user == null ? null : ToDto(user);
    }

    public async Task Create(CreateUserDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            PasswordHash = HashPassword(dto.Password),
            FullName = dto.FullName,
            RoleId = dto.RoleId
        };
        await _repository.Create(user);
    }

    public async Task Update(int id, UpdateUserDto dto)
    {
        var user = await _repository.GetById(id);
        user.Username = dto.Username;
        user.FullName = dto.FullName;
        user.RoleId = dto.RoleId;
        await _repository.Update(user);
    }

    public async Task Delete(int id) => await _repository.Delete(id);

    private static UserResponseDto ToDto(User u) =>
        new(u.UserId, u.Username, u.FullName, u.RoleId, u.Role?.RoleName);

    private static string HashPassword(string password)
    {
        using SHA256 sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}