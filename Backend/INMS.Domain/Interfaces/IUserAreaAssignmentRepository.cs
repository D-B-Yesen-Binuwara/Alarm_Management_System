using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces
{
    public interface IUserAreaAssignmentRepository
    {
        Task<UserAreaAssignment?> GetByUserId(int userId);
        Task Create(UserAreaAssignment assignment);
    }
}
