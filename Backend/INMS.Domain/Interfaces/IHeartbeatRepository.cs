using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces;

public interface IHeartbeatRepository
{
    Task<Heartbeat> AddAsync(Heartbeat heartbeat);
    Task<List<Heartbeat>> GetByDeviceIdAsync(int deviceId);
    Task<Heartbeat> GetLatestByDeviceIdAsync(int deviceId);
    Task<Dictionary<int, Heartbeat>> GetLatestHeartbeatsForAllDevicesAsync();
}