using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}