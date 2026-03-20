namespace INMS.Domain.Entities;

public class UserAreaAssignment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string AreaType { get; set; }

    public int AreaId { get; set; }

    public User User { get; set; }
}