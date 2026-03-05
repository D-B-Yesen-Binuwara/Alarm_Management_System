using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INMS.Domain.Entities
{
    [Table("DeviceLink")]
    public class DeviceLink
    {
        [Key]
        public int LinkId { get; set; }

        public int ParentDeviceId { get; set; }
        public int ChildDeviceId { get; set; }

        public string LinkStatus { get; set; }
    }
}