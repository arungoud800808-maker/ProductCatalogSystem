using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.Application.Queries.Products.GetProductById;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Repositories.UnitOfWork;
using ProductService.Services.Cache;
using Xunit;

namespace ProductService.Tests.Handlers;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRedisCacheService> _cacheMock;

    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IRedisCacheService>();

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(_productRepositoryMock.Object);

        _handler = new GetProductByIdQueryHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProductExistsInRedis_ShouldReturnCachedProduct()
    {
        // Arrange

        var query = new GetProductByIdQuery(1);

        var cachedProduct = new ProductDto
        {
            Id = 1,
            Name = "Laptop",
            Price = 50000
        };

        _cacheMock
            .Setup(x => x.GetAsync<ProductDto>("Product_1"))
            .ReturnsAsync(cachedProduct);

        // Act

        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Laptop");

        _productRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<int>()),
            Times.Never);

        _mapperMock.Verify(
            x => x.Map<ProductDto>(It.IsAny<Product>()),
            Times.Never);

        _cacheMock.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<ProductDto>(),
                It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProductNotInRedisButExistsInDatabase_ShouldReturnProductAndCacheIt()
    {
        // Arrange

        var query = new GetProductByIdQuery(1);

        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 50000
        };

        var dto = new ProductDto
        {
            Id = 1,
            Name = "Laptop",
            Price = 50000
        };

        _cacheMock
            .Setup(x => x.GetAsync<ProductDto>("Product_1"))
            .ReturnsAsync((ProductDto?)null);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(product))
            .Returns(dto);

        _cacheMock
            .Setup(x => x.SetAsync(
                "Product_1",
                dto,
                It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act

        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);

        _productRepositoryMock.Verify(
            x => x.GetByIdAsync(1),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<ProductDto>(product),
            Times.Once);

        _cacheMock.Verify(
            x => x.SetAsync(
                "Product_1",
                dto,
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange

        var query = new GetProductByIdQuery(100);

        _cacheMock
            .Setup(x => x.GetAsync<ProductDto>("Product_100"))
            .ReturnsAsync((ProductDto?)null);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(100))
            .ReturnsAsync((Product?)null);

        // Act

        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert

        result.Should().BeNull();

        _mapperMock.Verify(
            x => x.Map<ProductDto>(It.IsAny<Product>()),
            Times.Never);

        _cacheMock.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<ProductDto>(),
                It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryOnlyOnce_WhenCacheMissOccurs()
    {
        // Arrange

        var product = new Product { Id = 1 };

        _cacheMock
            .Setup(x => x.GetAsync<ProductDto>("Product_1"))
            .ReturnsAsync((ProductDto?)null);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(product))
            .Returns(new ProductDto());

        _cacheMock
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<ProductDto>(),
                It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act

        await _handler.Handle(
            new GetProductByIdQuery(1),
            CancellationToken.None);

        // Assert

        _productRepositoryMock.Verify(
            x => x.GetByIdAsync(1),
            Times.Once);
    }
}