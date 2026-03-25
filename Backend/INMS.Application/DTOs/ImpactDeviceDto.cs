namespace INMS.Application.DTOs
{
    public class ImpactDeviceDto
    {
        public int DeviceId { get; set; }

        public string? DeviceName { get; set; }

        public string? DeviceType { get; set; }

        public string? IP { get; set; }

        public string? PriorityLevel { get; set; }

        public int? LEAId { get; set; }

        public int? AssignedUserId { get; set; }
    }
}