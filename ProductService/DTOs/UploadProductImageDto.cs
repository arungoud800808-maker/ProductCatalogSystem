using Microsoft.AspNetCore.Http;

namespace ProductService.DTOs;

public class UploadProductImageDto
{
    public IFormFile Image { get; set; } = null!;
}