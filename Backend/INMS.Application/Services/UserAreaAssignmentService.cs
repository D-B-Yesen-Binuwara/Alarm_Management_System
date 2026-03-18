using INMS.Application.DTOs;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;

namespace INMS.Application.Services;

public class UserAreaAssignmentService
{
    private readonly IUserAreaAssignmentRepository _repository;

    public UserAreaAssignmentService(IUserAreaAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserAreaAssignment>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<UserAreaAssignment?> GetByUserIdAsync(int userId)
        => await _repository.GetByUserId(userId);

    public async Task AssignArea(AssignAreaDto dto)
    {
        if (dto.AreaType != "Region" && dto.AreaType != "Province" && dto.AreaType != "LEA")
            throw new Exception("Invalid AreaType. Must be Region, Province, or LEA.");

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
