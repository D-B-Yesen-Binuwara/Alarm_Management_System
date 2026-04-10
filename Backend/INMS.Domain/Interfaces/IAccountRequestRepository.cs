using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces;

public interface IAccountRequestRepository
{
    Task Create(AccountRequest request);
}
