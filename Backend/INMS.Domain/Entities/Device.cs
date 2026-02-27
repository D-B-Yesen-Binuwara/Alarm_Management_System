using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [MaxLength(20)]
        public string Status { get; set; } = "UP";

        [Required]
        [MaxLength(20)]
        public string PriorityLevel { get; set; } = "Low";

        [ForeignKey("LEA")]
        public int LEAId { get; set; }

        public int? AssignedUserId { get; set; }
    }
}