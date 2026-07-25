using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.Application.Commands.Products.UpdateProduct;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Repositories.UnitOfWork;
using Xunit;

namespace ProductService.Tests.Handlers;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(_productRepositoryMock.Object);

        _handler = new UpdateProductCommandHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProductExists_ShouldUpdateProductSuccessfully()
    {
        // Arrange

        var product = new Product
        {
            Id = 1,
            Name = "Old Laptop",
            Description = "Old Description",
            Price = 40000,
            Stock = 5,
            ImageUrl = "old.jpg",
            CategoryId = 1
        };

        var updateDto = new UpdateProductDto
        {
            Name = "Gaming Laptop",
            Description = "Updated Description",
            Price = 55000,
            Stock = 10,
            ImageUrl = "new.jpg",
            CategoryId = 2
        };

        var command = new UpdateProductCommand(1, updateDto);

        var productDto = new ProductDto
        {
            Id = 1,
            Name = "Gaming Laptop",
            Description = "Updated Description",
            Price = 55000,
            Stock = 10,
            ImageUrl = "new.jpg",
            CategoryId = 2
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.UpdateAsync(product))
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

        result!.Name.Should().Be("Gaming Laptop");
        result.Price.Should().Be(55000);
        result.Stock.Should().Be(10);

        product.Name.Should().Be("Gaming Laptop");
        product.Description.Should().Be("Updated Description");
        product.Price.Should().Be(55000);
        product.Stock.Should().Be(10);
        product.ImageUrl.Should().Be("new.jpg");
        product.CategoryId.Should().Be(2);

        _productRepositoryMock.Verify(
            x => x.GetByIdAsync(1),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.UpdateAsync(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<ProductDto>(product),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange

        var dto = new UpdateProductDto
        {
            Name = "Laptop",
            Description = "Desc",
            Price = 1000,
            Stock = 5,
            CategoryId = 1
        };

        var command = new UpdateProductCommand(100, dto);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(100))
            .ReturnsAsync((Product?)null);

        // Act

        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert

        result.Should().BeNull();

        _productRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Product>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);

        _mapperMock.Verify(
            x => x.Map<ProductDto>(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryUpdateOnce()
    {
        // Arrange

        var product = new Product { Id = 1 };

        var dto = new UpdateProductDto
        {
            Name = "Updated",
            Description = "Updated",
            Price = 500,
            Stock = 5,
            CategoryId = 1
        };

        var command = new UpdateProductCommand(1, dto);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.UpdateAsync(product))
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
            x => x.UpdateAsync(product),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesOnce()
    {
        // Arrange

        var product = new Product { Id = 1 };

        var dto = new UpdateProductDto
        {
            Name = "Updated",
            Description = "Updated",
            Price = 500,
            Stock = 5,
            CategoryId = 1
        };

        var command = new UpdateProductCommand(1, dto);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.UpdateAsync(product))
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