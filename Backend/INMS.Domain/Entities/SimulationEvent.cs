namespace INMS.Domain.Entities;

public class SimulationEvent
{
    public int EventId { get; set; }
    public int DeviceId { get; set; }
    public string EventType { get; set; }
    public DateTime EventTime { get; set; }
}
