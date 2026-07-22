namespace ProductService.DTOs;

public class BulkProductDto
{
    public List<CreateProductDto> Products { get; set; } = new();
}