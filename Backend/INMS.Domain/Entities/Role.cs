using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities;

public class Role
{
    [Key]
    public int RoleId { get; set; }
    public string RoleName { get; set; }
}
