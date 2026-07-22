using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class CreateReviewDto
{
    public int ProductId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }
}