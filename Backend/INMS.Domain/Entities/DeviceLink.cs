namespace INMS.Domain.Entities;

public class DeviceLink
{
    public int LinkId { get; set; }
    public int ParentDeviceId { get; set; }
    public int ChildDeviceId { get; set; }
    public string LinkStatus { get; set; }
}
