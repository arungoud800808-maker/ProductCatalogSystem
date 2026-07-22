using ProductService.DTOs;

namespace ProductService.Services;

public interface IReviewService
{
    Task AddReviewAsync(CreateReviewDto dto);

    Task<IEnumerable<ReviewDto>> GetReviewsAsync(int productId);

    Task<double> GetAverageRatingAsync(int productId);

    Task<int> GetReviewCountAsync(int productId);
}