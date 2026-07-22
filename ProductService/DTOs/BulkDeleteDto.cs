namespace ProductService.DTOs;

public class BulkDeleteDto
{
    public List<int> ProductIds { get; set; } = new();
}