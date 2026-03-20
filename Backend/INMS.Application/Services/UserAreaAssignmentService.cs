using INMS.Application.DTOs;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;

namespace INMS.Application.Services;

public class UserAreaAssignmentService
{
    private readonly IUserAreaAssignmentRepository _repository;
    private readonly IUserRepository _userRepository;

    private static readonly Dictionary<string, string> RoleAreaMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Region Officer",   "Region"   },
        { "Province Officer", "Province" },
        { "LEA Officer",      "LEA"      }
    };

    public UserAreaAssignmentService(
        IUserAreaAssignmentRepository repository,
        IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<List<UserAreaAssignment>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<UserAreaAssignment?> GetByUserIdAsync(int userId)
        => await _repository.GetByUserId(userId);

    public async Task AssignArea(AssignAreaDto dto)
    {
        var user = await _userRepository.GetById(dto.UserId)
            ?? throw new Exception("User not found.");

        var roleName = user.Role?.RoleName
            ?? throw new Exception("User has no role assigned.");

        if (roleName == "Admin")
            throw new Exception("Admin has no area restriction and cannot be assigned to an area.");

        if (!RoleAreaMap.TryGetValue(roleName, out var expectedAreaType))
            throw new Exception($"Unknown role '{roleName}'.");

        if (dto.AreaType != expectedAreaType)
            throw new Exception($"Role '{roleName}' can only be assigned to a '{expectedAreaType}' area.");

        var assignment = new UserAreaAssignment
        {
            UserId = dto.UserId,
            AreaType = dto.AreaType,
            AreaId = dto.AreaId
        };

        await _repository.Create(assignment);
    }

    public async Task DeleteAsync(int assignmentId)
    {
        var assignment = await _repository.GetByIdAsync(assignmentId)
            ?? throw new Exception("Assignment not found.");
        await _repository.DeleteAsync(assignment);
    }
}
