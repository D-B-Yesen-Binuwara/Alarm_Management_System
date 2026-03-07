using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INMS.Domain.Entities
{
    [Table("Device")]
    public class Device
    {
        [Key]
        public int DeviceId { get; set; }

        public string DeviceName { get; set; }

        public string DeviceType { get; set; }

        public string IP { get; set; }

        public string Status { get; set; }

        public string PriorityLevel { get; set; }

        public int? LEAId { get; set; }           // nullable

        public int? AssignedUserId { get; set; }  // nullable
    }
}