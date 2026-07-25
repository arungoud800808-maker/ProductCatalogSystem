using FluentAssertions;
using Moq;
using ProductService.Application.Commands.Products.DeleteProduct;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Repositories.UnitOfWork;
using Xunit;

namespace ProductService.Tests.Handlers;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;

    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(_productRepositoryMock.Object);

        _handler = new DeleteProductCommandHandler(
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProductExists_ShouldDeleteProductAndReturnTrue()
    {
        // Arrange

        var product = new Product
        {
            Id = 1,
            Name = "Laptop"
        };

        var command = new DeleteProductCommand(1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act

        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert

        result.Should().BeTrue();

        _productRepositoryMock.Verify(
            x => x.GetByIdAsync(1),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.DeleteAsync(1),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldReturnFalse()
    {
        // Arrange

        var command = new DeleteProductCommand(100);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(100))
            .ReturnsAsync((Product?)null);

        // Act

        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert

        result.Should().BeFalse();

        _productRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<int>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteAsyncOnce()
    {
        // Arrange

        var product = new Product
        {
            Id = 1
        };

        var command = new DeleteProductCommand(1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act

        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert

        _productRepositoryMock.Verify(
            x => x.DeleteAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesOnce()
    {
        // Arrange

        var product = new Product
        {
            Id = 1
        };

        var command = new DeleteProductCommand(1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act

        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }
}