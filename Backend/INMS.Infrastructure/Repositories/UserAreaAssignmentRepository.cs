using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INMS.Infrastructure.Repositories;

public class UserAreaAssignmentRepository : IUserAreaAssignmentRepository
{
    private readonly AppDbContext _context;

    public UserAreaAssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserAreaAssignment>> GetAllAsync()
        => await _context.UserAreaAssignments.ToListAsync();

    public async Task<UserAreaAssignment?> GetByUserId(int userId)
        => await _context.UserAreaAssignments.FirstOrDefaultAsync(x => x.UserId == userId);

    public async Task<UserAreaAssignment?> GetByIdAsync(int assignmentId)
        => await _context.UserAreaAssignments.FindAsync(assignmentId);

    public async Task Create(UserAreaAssignment assignment)
    {
        _context.UserAreaAssignments.Add(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserAreaAssignment assignment)
    {
        _context.UserAreaAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
    }
}
