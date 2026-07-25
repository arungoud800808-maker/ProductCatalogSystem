using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests.Services;

public class WishlistServiceTests
{
    private readonly Mock<IWishlistRepository> _wishlistRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly WishlistService _service;

    public WishlistServiceTests()
    {
        _wishlistRepositoryMock = new Mock<IWishlistRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();

        _service = new WishlistService(
            _wishlistRepositoryMock.Object,
            _productRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task AddAsync_WhenProductExists_ShouldAddWishlist()
    {
        // Arrange

        int userId = 1;

        var dto = new CreateWishlistDto
        {
            ProductId = 10
        };

        var product = new Product
        {
            Id = 10,
            Name = "Laptop",
            Price = 50000,
            ImageUrl = "laptop.jpg"
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.ProductId))
            .ReturnsAsync(product);

        _wishlistRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Wishlist>()))
            .Returns(Task.CompletedTask);

        // Act

        await _service.AddAsync(userId, dto);

        // Assert

        _productRepositoryMock.Verify(
            x => x.GetByIdAsync(dto.ProductId),
            Times.Once);

        _wishlistRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Wishlist>(w =>
                w.UserId == userId &&
                w.ProductId == dto.ProductId)),
            Times.Once);
    }

    [Fact]
    public async Task AddAsync_WhenProductDoesNotExist_ShouldThrowException()
    {
        // Arrange

        var dto = new CreateWishlistDto
        {
            ProductId = 99
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.ProductId))
            .ReturnsAsync((Product?)null);

        // Act

        Func<Task> action = async () =>
            await _service.AddAsync(1, dto);

        // Assert

        await action.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Product not found.");

        _wishlistRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Wishlist>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByUserAsync_ShouldReturnWishlistItems()
    {
        // Arrange

        int userId = 1;

        var wishlists = new List<Wishlist>
        {
            new Wishlist
            {
                Id = 1,
                UserId = userId,
                ProductId = 10,
                CreatedDate = DateTime.UtcNow,
                Product = new Product
                {
                    Id = 10,
                    Name = "Laptop",
                    Price = 50000,
                    ImageUrl = "laptop.jpg"
                }
            },
            new Wishlist
            {
                Id = 2,
                UserId = userId,
                ProductId = 20,
                CreatedDate = DateTime.UtcNow,
                Product = new Product
                {
                    Id = 20,
                    Name = "Mouse",
                    Price = 1000,
                    ImageUrl = "mouse.jpg"
                }
            }
        };

        _wishlistRepositoryMock
            .Setup(x => x.GetByUserAsync(userId))
            .ReturnsAsync(wishlists);

        // Act

        var result = await _service.GetByUserAsync(userId);

        // Assert

        result.Should().HaveCount(2);

        result.First().ProductName.Should().Be("Laptop");

        result.First().Price.Should().Be(50000);

        _wishlistRepositoryMock.Verify(
            x => x.GetByUserAsync(userId),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallRepository()
    {
        // Arrange

        int wishlistId = 1;

        _wishlistRepositoryMock
            .Setup(x => x.RemoveAsync(wishlistId))
            .Returns(Task.CompletedTask);

        // Act

        await _service.RemoveAsync(wishlistId);

        // Assert

        _wishlistRepositoryMock.Verify(
            x => x.RemoveAsync(wishlistId),
            Times.Once);
    }
}