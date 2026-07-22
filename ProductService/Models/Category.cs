using ProductService.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Models;

public class Category:ISoftDelete
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Navigation Property
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}