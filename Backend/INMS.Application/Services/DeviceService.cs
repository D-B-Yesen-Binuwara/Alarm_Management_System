using INMS.Domain.Entities;
using INMS.Domain.Interfaces;

namespace INMS.Application.Services;

public class DeviceService
{
    private readonly IDeviceRepository _repository;

    public DeviceService(IDeviceRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Device>> GetAllDevices()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Device> GetDeviceById(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
