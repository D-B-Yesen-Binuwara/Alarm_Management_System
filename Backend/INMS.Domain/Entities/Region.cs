using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities
{
    public class Region
    {
        public int RegionId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
