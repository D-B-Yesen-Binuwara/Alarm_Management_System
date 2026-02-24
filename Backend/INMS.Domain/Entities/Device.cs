using INMS.Domain.Enums;

namespace INMS.Domain.Entities;

public class Device
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; }
    public DeviceType DeviceType { get; set; }
    public string IP { get; set; }
    public DeviceStatus Status { get; set; }
    public PriorityLevel PriorityLevel { get; set; }
    public int LEAId { get; set; }
    public int? AssignedUserId { get; set; }
}
