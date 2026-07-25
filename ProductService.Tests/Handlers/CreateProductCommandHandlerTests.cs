using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.Application.Commands.Products.CreateProduct;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Repositories.UnitOfWork;
using Xunit;

namespace ProductService.Tests.Handlers;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(_productRepositoryMock.Object);

        _handler = new CreateProductCommandHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateProductSuccessfully()
    {
        // Arrange

        var createDto = new CreateProductDto
        {
            Name = "Laptop",
            Description = "Gaming Laptop",
            Price = 50000,
            Stock = 10,
            CategoryId = 1,
            ImageUrl = "laptop.jpg"
        };

        var product = new Product
        {
            Id = 1,
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            Stock = createDto.Stock,
            CategoryId = createDto.CategoryId,
            ImageUrl = createDto.ImageUrl
        };

        var productDto = new ProductDto
        {
            Id = 1,
            Name = "Laptop",
            Description = "Gaming Laptop",
            Price = 50000,
            Stock = 10,
            CategoryId = 1,
            ImageUrl = "laptop.jpg"
        };

        var command = new CreateProductCommand(createDto);

        _mapperMock
            .Setup(x => x.Map<Product>(createDto))
            .Returns(product);

        _productRepositoryMock
            .Setup(x => x.AddAsync(product))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(product))
            .Returns(productDto);

        // Act

        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert

        result.Should().NotBeNull();
        result.Name.Should().Be("Laptop");
        result.Price.Should().Be(50000);

        _mapperMock.Verify(
            x => x.Map<Product>(createDto),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.AddAsync(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<ProductDto>(product),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryOnce()
    {
        // Arrange

        var dto = new CreateProductDto
        {
            Name = "Mobile",
            Description = "Android Phone",
            Price = 25000,
            Stock = 5,
            CategoryId = 2
        };

        var product = new Product();

        var command = new CreateProductCommand(dto);

        _mapperMock
            .Setup(x => x.Map<Product>(dto))
            .Returns(product);

        _productRepositoryMock
            .Setup(x => x.AddAsync(product))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(product))
            .Returns(new ProductDto());

        // Act

        await _handler.Handle(command, CancellationToken.None);

        // Assert

        _productRepositoryMock.Verify(
            x => x.AddAsync(product),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesOnce()
    {
        // Arrange

        var dto = new CreateProductDto
        {
            Name = "Keyboard",
            Price = 1000,
            Stock = 15
        };

        var product = new Product();

        var command = new CreateProductCommand(dto);

        _mapperMock
            .Setup(x => x.Map<Product>(dto))
            .Returns(product);

        _productRepositoryMock
            .Setup(x => x.AddAsync(product))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(product))
            .Returns(new ProductDto());

        // Act

        await _handler.Handle(command, CancellationToken.None);

        // Assert

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }
}