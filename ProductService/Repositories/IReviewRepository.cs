using ProductService.Models;

namespace ProductService.Repositories;

public interface IReviewRepository
{
    Task AddReviewAsync(Review review);

    Task<IEnumerable<Review>> GetReviewsByProductAsync(int productId);

    Task<double> GetAverageRatingAsync(int productId);

    Task<int> GetReviewCountAsync(int productId);
}