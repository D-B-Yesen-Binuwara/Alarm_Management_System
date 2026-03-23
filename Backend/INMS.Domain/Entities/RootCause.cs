using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities;

public class RootCause
{
    [Key]
    public int RootCauseId { get; set; }
    public int AlarmId { get; set; }
    public int RootCauseDeviceId { get; set; }
    public string RootCauseType { get; set; } = string.Empty;
    public DateTime DetectedTime { get; set; }
}
