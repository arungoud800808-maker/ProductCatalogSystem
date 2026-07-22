using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Constants;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewController(IReviewService service)
    {
        _service = service;
    }

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    public async Task<IActionResult> AddReview(CreateReviewDto dto)
    {
        await _service.AddReviewAsync(dto);

        return Ok("Review Added Successfully");
    }

    [AllowAnonymous]
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetReviews(int productId)
    {
        var reviews = await _service.GetReviewsAsync(productId);

        return Ok(reviews);
    }

    [AllowAnonymous]
    [HttpGet("{productId}/rating")]
    public async Task<IActionResult> GetRating(int productId)
    {
        var average = await _service.GetAverageRatingAsync(productId);

        var count = await _service.GetReviewCountAsync(productId);

        return Ok(new
        {
            AverageRating = average,
            TotalReviews = count
        });
    }
}