using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductService.Controllers;
using ProductService.DTOs;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests.Controllers;

public class ReviewControllerTests
{
    private readonly Mock<IReviewService> _serviceMock;
    private readonly ReviewController _controller;

    public ReviewControllerTests()
    {
        _serviceMock = new Mock<IReviewService>();

        _controller = new ReviewController(
            _serviceMock.Object);
    }

    [Fact]
    public async Task AddReview_ShouldReturnOk()
    {
        // Arrange

        var dto = new CreateReviewDto
        {
            ProductId = 1,
            Rating = 5,
            Comment = "Excellent Product"
        };

        _serviceMock
            .Setup(x => x.AddReviewAsync(dto))
            .Returns(Task.CompletedTask);

        // Act

        var result = await _controller.AddReview(dto);

        // Assert

        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        okResult.Value.Should().Be("Review Added Successfully");

        _serviceMock.Verify(
            x => x.AddReviewAsync(dto),
            Times.Once);
    }

    [Fact]
    public async Task GetReviews_ShouldReturnOk()
    {
        // Arrange

        int productId = 1;

        var reviews = new List<ReviewDto>
{
    new ReviewDto
    {
        Id = 1,
        Rating = 5,
        Comment = "Excellent",
        CreatedDate = DateTime.UtcNow
    },
    new ReviewDto
    {
        Id = 2,
        Rating = 4,
        Comment = "Very Good",
        CreatedDate = DateTime.UtcNow
    }
};

        _serviceMock
            .Setup(x => x.GetReviewsAsync(productId))
            .ReturnsAsync(reviews);

        // Act

        var result = await _controller.GetReviews(productId);

        // Assert

        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var value = okResult.Value.Should()
            .BeAssignableTo<IEnumerable<ReviewDto>>()
            .Subject;

        value.Should().HaveCount(2);

        value.First().Rating.Should().Be(5);

        _serviceMock.Verify(
            x => x.GetReviewsAsync(productId),
            Times.Once);
    }

    [Fact]
    public async Task GetRating_ShouldReturnAverageRatingAndReviewCount()
    {
        // Arrange

        int productId = 1;

        _serviceMock
            .Setup(x => x.GetAverageRatingAsync(productId))
            .ReturnsAsync(4.5);

        _serviceMock
            .Setup(x => x.GetReviewCountAsync(productId))
            .ReturnsAsync(20);

        // Act

        var result = await _controller.GetRating(productId);

        // Assert

        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var value = okResult.Value!;

        var averageRating = value
            .GetType()
            .GetProperty("AverageRating")!
            .GetValue(value);

        var totalReviews = value
            .GetType()
            .GetProperty("TotalReviews")!
            .GetValue(value);

        averageRating.Should().Be(4.5);

        totalReviews.Should().Be(20);

        _serviceMock.Verify(
            x => x.GetAverageRatingAsync(productId),
            Times.Once);

        _serviceMock.Verify(
            x => x.GetReviewCountAsync(productId),
            Times.Once);
    }
}