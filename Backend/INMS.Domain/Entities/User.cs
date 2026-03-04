using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities;

public class User
{
    [Key]
    public int UserId { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string FullName { get; set; }
    public int RoleId { get; set; }
}
