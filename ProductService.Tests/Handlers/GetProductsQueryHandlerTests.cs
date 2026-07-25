using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.Application.Queries.Products.GetProducts;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Repositories.Generic;
using ProductService.Repositories.UnitOfWork;
using Xunit;

namespace ProductService.Tests.Handlers;

public class GetProductsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Product>> _genericRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _genericRepositoryMock = new Mock<IGenericRepository<Product>>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock
            .Setup(x => x.ProductGeneric)
            .Returns(_genericRepositoryMock.Object);

        _handler = new GetProductsQueryHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProductsSuccessfully()
    {
        // Arrange

        var products = new List<Product>
        {
            new()
            {
                Id = 1,
                Name = "Laptop",
                Price = 50000
            },
            new()
            {
                Id = 2,
                Name = "Mouse",
                Price = 500
            }
        };

        var productDtos = new List<ProductDto>
        {
            new()
            {
                Id = 1,
                Name = "Laptop",
                Price = 50000
            },
            new()
            {
                Id = 2,
                Name = "Mouse",
                Price = 500
            }
        };

        _genericRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(products);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);

        // Act

        var result = await _handler.Handle(
            new GetProductsQuery(),
            CancellationToken.None);

        // Assert

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _genericRepositoryMock.Verify(
            x => x.GetAllAsync(),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<IEnumerable<ProductDto>>(products),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoProductsExist_ShouldReturnEmptyCollection()
    {
        // Arrange

        var products = new List<Product>();

        var dtos = new List<ProductDto>();

        _genericRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(products);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(dtos);

        // Act

        var result = await _handler.Handle(
            new GetProductsQuery(),
            CancellationToken.None);

        // Assert

        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _genericRepositoryMock.Verify(
            x => x.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryOnlyOnce()
    {
        // Arrange

        _genericRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Product>());

        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns(new List<ProductDto>());

        // Act

        await _handler.Handle(
            new GetProductsQuery(),
            CancellationToken.None);

        // Assert

        _genericRepositoryMock.Verify(
            x => x.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallMapperOnlyOnce()
    {
        // Arrange

        var products = new List<Product>();

        _genericRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(products);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(new List<ProductDto>());

        // Act

        await _handler.Handle(
            new GetProductsQuery(),
            CancellationToken.None);

        // Assert

        _mapperMock.Verify(
            x => x.Map<IEnumerable<ProductDto>>(products),
            Times.Once);
    }
}