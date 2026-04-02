using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities;

public class UserAreaAssignment
{
    [Key]
    public int AssignmentId { get; set; }

    public int UserId { get; set; }

    public string AreaType { get; set; } = string.Empty;

    public int AreaId { get; set; }

    public User? User { get; set; }
}