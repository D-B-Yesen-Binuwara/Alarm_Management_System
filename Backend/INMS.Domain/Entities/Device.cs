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
        [MaxLength(20)]
        public string Status { get; set; } = "UP";

        [Required]
        [MaxLength(20)]
        public PriorityLevel PriorityLevel { get; set; } = PriorityLevel.Low;

        [ForeignKey("LEA")]
        public int LEAId { get; set; }

        public int? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }
    }
}