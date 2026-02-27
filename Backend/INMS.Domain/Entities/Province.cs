using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities;

public class Province
{
    [Key]
    public int ProvinceId { get; set; }
    public string Name { get; set; }
    public int RegionId { get; set; }
}
