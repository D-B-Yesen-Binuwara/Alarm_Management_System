using INMS.Application.DTOs;

namespace INMS.Application.Interfaces;

public interface IAccountRequestService
{
    Task Submit(CreateAccountRequestDto dto);
}
