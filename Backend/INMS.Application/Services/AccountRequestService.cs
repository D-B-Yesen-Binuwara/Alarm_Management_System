using INMS.Application.DTOs;
using INMS.Application.Interfaces;
using INMS.Domain.Entities;
using INMS.Domain.Interfaces;

namespace INMS.Application.Services;

public class AccountRequestService : IAccountRequestService
{
    private readonly IAccountRequestRepository _repository;

    public AccountRequestService(IAccountRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task Submit(CreateAccountRequestDto dto)
    {
        var request = new AccountRequest
        {
            FullName = dto.FullName,
            Email = dto.Email,
            ServiceId = dto.ServiceId,
            RoleId = dto.RoleId,
            RegionId = dto.RegionId,
            ProvinceId = dto.ProvinceId,
            LEAId = dto.LEAId
        };

        await _repository.Create(request);
    }
}
