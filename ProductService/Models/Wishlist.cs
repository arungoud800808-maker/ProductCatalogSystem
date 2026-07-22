namespace ProductService.Models;

public class Wishlist
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public ICollection<Wishlist> Wishlists { get; set; }
    = new List<Wishlist>();
}