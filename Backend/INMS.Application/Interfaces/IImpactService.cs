using INMS.Application.DTOs;

namespace INMS.Application.Interfaces
{
    public interface IImpactService
    {
        Task<List<ImpactDeviceDto>> GetImpactedDevices(int deviceId);
    }
}