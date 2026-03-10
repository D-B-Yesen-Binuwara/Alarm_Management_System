using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using INMS.Domain.Enums;

namespace INMS.Domain.Entities
{
    public class Device
    {
        [Key]
        public int DeviceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DeviceName { get; set; } = string.Empty;

        [Required]
        public DeviceType DeviceType { get; set; }

        [Required]
        [MaxLength(50)]
        public string IP { get; set; } = string.Empty;

        [Required]
        public DeviceStatus Status { get; set; } = DeviceStatus.UP;

        [Required]
        public PriorityLevel PriorityLevel { get; set; } = PriorityLevel.LOW;

        [ForeignKey("LEA")]
        public int LEAId { get; set; }

        public int? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }
    }
}