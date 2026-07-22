namespace ProductService.DTOs;

public class WishlistDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedDate { get; set; }
}