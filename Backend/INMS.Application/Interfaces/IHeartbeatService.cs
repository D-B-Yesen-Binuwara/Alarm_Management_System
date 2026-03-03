using INMS.Domain.Entities;

namespace INMS.Application.Interfaces;

public interface IHeartbeatService
{
    Task<Heartbeat> RecordHeartbeatAsync(int deviceId, string status);
    Task<List<Heartbeat>> GetDeviceHeartbeatsAsync(int deviceId);
    Task<Heartbeat> GetLatestHeartbeatAsync(int deviceId);
}
