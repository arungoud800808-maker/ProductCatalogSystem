using AutoMapper;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _repository;
    private readonly IMapper _mapper;

    public ReviewService(
        IReviewRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task AddReviewAsync(CreateReviewDto dto)
    {
        var review = new Review
        {
            ProductId = dto.ProductId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        await _repository.AddReviewAsync(review);
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsAsync(int productId)
    {
        var reviews = await _repository.GetReviewsByProductAsync(productId);

        return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
    }

    public async Task<double> GetAverageRatingAsync(int productId)
    {
        return await _repository.GetAverageRatingAsync(productId);
    }

    public async Task<int> GetReviewCountAsync(int productId)
    {
        return await _repository.GetReviewCountAsync(productId);
    }
}