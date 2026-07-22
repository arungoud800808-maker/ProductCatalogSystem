using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ProductDbContext _context;

    public ReviewRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task AddReviewAsync(Review review)
    {
        _context.Reviews.Add(review);

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByProductAsync(int productId)
    {
        return await _context.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingAsync(int productId)
    {
        var reviews = _context.Reviews.Where(r => r.ProductId == productId);

        if (!await reviews.AnyAsync())
            return 0;

        return await reviews.AverageAsync(r => r.Rating);
    }

    public async Task<int> GetReviewCountAsync(int productId)
    {
        return await _context.Reviews
            .CountAsync(r => r.ProductId == productId);
    }
}