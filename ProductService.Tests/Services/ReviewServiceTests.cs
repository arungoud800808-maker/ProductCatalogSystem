using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests.Services;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ReviewService _service;

    public ReviewServiceTests()
    {
        _repositoryMock = new Mock<IReviewRepository>();
        _mapperMock = new Mock<IMapper>();

        _service = new ReviewService(
            _repositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task AddReviewAsync_ShouldAddReview()
    {
        // Arrange

        var dto = new CreateReviewDto
        {
            ProductId = 1,
            Rating = 5,
            Comment = "Excellent Product"
        };

        _repositoryMock
            .Setup(x => x.AddReviewAsync(It.IsAny<Review>()))
            .Returns(Task.CompletedTask);

        // Act

        await _service.AddReviewAsync(dto);

        // Assert

        _repositoryMock.Verify(
            x => x.AddReviewAsync(It.Is<Review>(r =>
                r.ProductId == dto.ProductId &&
                r.Rating == dto.Rating &&
                r.Comment == dto.Comment)),
            Times.Once);
    }

    [Fact]
    public async Task GetReviewsAsync_ShouldReturnReviews()
    {
        // Arrange

        int productId = 1;

        var reviews = new List<Review>
        {
            new()
            {
                Id = 1,
                ProductId = productId,
                Rating = 5,
                Comment = "Excellent"
            },
            new()
            {
                Id = 2,
                ProductId = productId,
                Rating = 4,
                Comment = "Very Good"
            }
        };

        var reviewDtos = new List<ReviewDto>
        {
            new()
            {
                Id = 1,
                Rating = 5,
                Comment = "Excellent",
                CreatedDate = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                Rating = 4,
                Comment = "Very Good",
                CreatedDate = DateTime.UtcNow
            }
        };

        _repositoryMock
            .Setup(x => x.GetReviewsByProductAsync(productId))
            .ReturnsAsync(reviews);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<ReviewDto>>(reviews))
            .Returns(reviewDtos);

        // Act

        var result = await _service.GetReviewsAsync(productId);

        // Assert

        result.Should().HaveCount(2);

        result.First().Rating.Should().Be(5);

        _repositoryMock.Verify(
            x => x.GetReviewsByProductAsync(productId),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<IEnumerable<ReviewDto>>(reviews),
            Times.Once);
    }

    [Fact]
    public async Task GetAverageRatingAsync_ShouldReturnAverageRating()
    {
        // Arrange

        int productId = 1;

        _repositoryMock
            .Setup(x => x.GetAverageRatingAsync(productId))
            .ReturnsAsync(4.5);

        // Act

        var result = await _service.GetAverageRatingAsync(productId);

        // Assert

        result.Should().Be(4.5);

        _repositoryMock.Verify(
            x => x.GetAverageRatingAsync(productId),
            Times.Once);
    }

    [Fact]
    public async Task GetReviewCountAsync_ShouldReturnReviewCount()
    {
        // Arrange

        int productId = 1;

        _repositoryMock
            .Setup(x => x.GetReviewCountAsync(productId))
            .ReturnsAsync(10);

        // Act

        var result = await _service.GetReviewCountAsync(productId);

        // Assert

        result.Should().Be(10);

        _repositoryMock.Verify(
            x => x.GetReviewCountAsync(productId),
            Times.Once);
    }
}