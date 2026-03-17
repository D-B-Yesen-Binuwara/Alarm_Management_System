using INMS.Domain.Entities;
using INMS.Domain.Repositories;

namespace INMS.Application.Services;

public class UserAreaAssignmentService
{
    private readonly IUserAreaAssignmentRepository _repository;

    public UserAreaAssignmentService(IUserAreaAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async Task AssignArea(int userId, string areaType, int areaId)
    {
        if (areaType != "Region" && areaType != "Province" && areaType != "LEA")
            throw new Exception("Invalid AreaType");

        var assignment = new UserAreaAssignment
        {
            UserId = userId,
            AreaType = areaType,
            AreaId = areaId
        };

        await _repository.Create(assignment);
    }
}
