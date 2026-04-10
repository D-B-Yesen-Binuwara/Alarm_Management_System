using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities;

public class Alarm
{
    [Key]
    public int AlarmId { get; set; }
    public int DeviceId { get; set; }
    public string AlarmType { get; set; } = string.Empty;
    public DateTime RaisedTime { get; set; }
    public DateTime? ClearedTime { get; set; }
    public bool IsActive { get; set; }
}
