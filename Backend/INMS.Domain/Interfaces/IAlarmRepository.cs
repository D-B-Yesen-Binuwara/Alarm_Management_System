using INMS.Domain.Entities;

namespace INMS.Domain.Interfaces;

public interface IAlarmRepository
{
    Task<Alarm> GetByIdAsync(int id);
    Task<List<Alarm>> GetAllAsync();
    Task<List<Alarm>> GetByDeviceIdAsync(int deviceId);
    Task<Alarm> AddAsync(Alarm alarm);
    Task<Alarm> UpdateAsync(Alarm alarm);
    Task<bool> DeleteAsync(int id);
}
