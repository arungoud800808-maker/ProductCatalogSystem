using ProductService.Interfaces;

namespace ProductService.Models;

public class Product : ISoftDelete
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }

    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<Review> Reviews { get; set; }
        = new List<Review>();

    public ICollection<Wishlist> Wishlists { get; set; }
        = new List<Wishlist>();

    // Soft Delete Properties
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}