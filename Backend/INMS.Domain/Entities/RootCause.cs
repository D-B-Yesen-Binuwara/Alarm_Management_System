namespace INMS.Domain.Entities;

public class RootCause
{
    public int RootCauseId { get; set; }
    public int AlarmId { get; set; }
    public int RootCauseDeviceId { get; set; }
    public string RootCauseType { get; set; }
    public DateTime DetectedTime { get; set; }
}
