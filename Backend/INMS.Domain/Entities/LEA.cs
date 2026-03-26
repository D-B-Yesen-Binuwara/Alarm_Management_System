using System.ComponentModel.DataAnnotations;

namespace INMS.Domain.Entities
{
    public class LEA
    {
        [Key]
        public int LEAId { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public int ProvinceId { get; set; }
        public Province? Province { get; set; }
    }
}