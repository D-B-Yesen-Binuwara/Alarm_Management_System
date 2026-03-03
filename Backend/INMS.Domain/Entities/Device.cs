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
        [MaxLength(50)]
        public string DeviceType { get; set; } = string.Empty;

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

        // FK to User (Officer)
        public int? AssignedUserId { get; set; }
    }
}