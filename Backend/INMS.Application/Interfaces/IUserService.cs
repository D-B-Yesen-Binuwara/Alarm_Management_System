using INMS.Application.DTOs;

namespace INMS.Application.Services;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAll();
    Task<UserResponseDto?> GetById(int id);
    Task Create(CreateUserDto dto);
    Task Update(int id, UpdateUserDto dto);
    Task Delete(int id);
}