using INMS.Domain.Entities;
using INMS.Domain.Interfaces;
using INMS.Application.Interfaces;

namespace INMS.Application.Services;

public class HeartbeatService : IHeartbeatService
{
    private readonly IHeartbeatRepository _repository;

    public HeartbeatService(IHeartbeatRepository repository)
    {
        _repository = repository;
    }

    public async Task<Heartbeat> RecordHeartbeatAsync(int deviceId, string status)
    {
        var heartbeat = new Heartbeat
        {
            DeviceId = deviceId,
            Status = status,
            Timestamp = DateTime.UtcNow
        };

        return await _repository.AddAsync(heartbeat);
    }

    public async Task<List<Heartbeat>> GetDeviceHeartbeatsAsync(int deviceId)
    {
        return await _repository.GetByDeviceIdAsync(deviceId);
    }

    public async Task<Heartbeat> GetLatestHeartbeatAsync(int deviceId)
    {
        return await _repository.GetLatestByDeviceIdAsync(deviceId);
    }
}
