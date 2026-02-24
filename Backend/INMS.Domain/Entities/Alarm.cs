namespace INMS.Domain.Entities;

public class Alarm
{
    public int AlarmId { get; set; }
    public int DeviceId { get; set; }
    public string AlarmType { get; set; }
    public DateTime RaisedTime { get; set; }
    public DateTime? ClearedTime { get; set; }
    public bool IsActive { get; set; }
}
