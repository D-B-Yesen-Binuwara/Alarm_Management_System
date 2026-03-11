using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities;

public class DeviceLink
{
    [Key]
    public int LinkId { get; set; }

    public int ParentDeviceId { get; set; }
    public Device? ParentDevice { get; set; }

    public int ChildDeviceId { get; set; }
    public Device? ChildDevice { get; set; }

    public string LinkStatus { get; set; } = "UP";
}