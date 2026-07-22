namespace ProductService.DTOs;

public class ReviewDto
{
    public int Id { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedDate { get; set; }
}