using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities;

public class Region
{
    [Key]
    public int RegionId { get; set; }
    public string Name { get; set; }
}
