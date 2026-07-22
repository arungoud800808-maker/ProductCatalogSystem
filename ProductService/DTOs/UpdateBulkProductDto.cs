namespace ProductService.DTOs;

public class UpdateBulkProductDto
{
    public List<UpdateProductDtoWithId> Products { get; set; } = new();
}

public class UpdateProductDtoWithId
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }
}