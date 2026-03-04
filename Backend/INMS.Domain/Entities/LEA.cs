using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities;

public class LEA
{
    [Key]
    public int LEAId { get; set; }
    public string Name { get; set; }
    public int ProvinceId { get; set; }
}
