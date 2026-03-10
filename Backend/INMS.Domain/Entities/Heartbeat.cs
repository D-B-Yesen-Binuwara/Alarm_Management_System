namespace INMS.Domain.Entities;

public class Heartbeat
{
    public int HeartbeatId { get; set; }
    public int DeviceId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; }
}
