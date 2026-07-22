using ProductService.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Models;

public class Review:ISoftDelete
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}