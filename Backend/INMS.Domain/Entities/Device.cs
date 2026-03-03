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
        public string DeviceName { get; set; }

        [Required]
        [MaxLength(50)]
        public string DeviceType { get; set; } // SLBN, CEAN, MSAN

        [Required]
        [MaxLength(50)]
        public string IP { get; set; }

        [Required]
        public DeviceStatus Status { get; set; } = DeviceStatus.UP;

        [Required]
        public PriorityLevel PriorityLevel { get; set; } = PriorityLevel.LOW;

        [ForeignKey("LEA")]
        public int LEAId { get; set; }

        public int? AssignedUserId { get; set; }
    }
}