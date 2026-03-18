using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces
{
    public interface IUserAreaAssignmentRepository
    {
        Task<List<UserAreaAssignment>> GetAllAsync();
        Task<UserAreaAssignment?> GetByUserId(int userId);
        Task<UserAreaAssignment?> GetByIdAsync(int assignmentId);
        Task Create(UserAreaAssignment assignment);
        Task DeleteAsync(UserAreaAssignment assignment);
    }
}
