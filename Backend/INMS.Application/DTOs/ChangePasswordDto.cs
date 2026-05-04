namespace INMS.Application.DTOs
{
    public class ChangePasswordDto
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; }
    }
}