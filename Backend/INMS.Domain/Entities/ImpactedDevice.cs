namespace INMS.Domain.Entities;

public class ImpactedDevice
{
    public int ImpactId { get; set; }
    public int RootCauseId { get; set; }
    public int DeviceId { get; set; }
    public string ImpactType { get; set; }
}
