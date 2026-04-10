using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Infrastructure.Persistence;

namespace INMS.Infrastructure.Repositories;

public class AccountRequestRepository : IAccountRequestRepository
{
    private readonly AppDbContext _context;

    public AccountRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task Create(AccountRequest request)
    {
        _context.AccountRequests.Add(request);
        await _context.SaveChangesAsync();
    }
}
