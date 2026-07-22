using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class UpdateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}