using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductService.Data;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Repositories.Generic;
using ProductService.Repositories.UnitOfWork;
using Xunit;

namespace ProductService.Tests.Repositories;

public class UnitOfWorkTests : IDisposable
{
    private readonly ProductDbContext _context;

    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<IWishlistRepository> _wishlistRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;

    private readonly Mock<IGenericRepository<Product>> _productGenericMock;
    private readonly Mock<IGenericRepository<Category>> _categoryGenericMock;
    private readonly Mock<IGenericRepository<User>> _userGenericMock;

    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ProductDbContext(options);

        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _wishlistRepositoryMock = new Mock<IWishlistRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();

        _productGenericMock = new Mock<IGenericRepository<Product>>();
        _categoryGenericMock = new Mock<IGenericRepository<Category>>();
        _userGenericMock = new Mock<IGenericRepository<User>>();

        _unitOfWork = new UnitOfWork(
            _context,
            _productRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _reviewRepositoryMock.Object,
            _wishlistRepositoryMock.Object,
            _productGenericMock.Object,
            _categoryGenericMock.Object,
            _userGenericMock.Object,
            _auditRepositoryMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void Constructor_ShouldInitializeRepositories()
    {
        _unitOfWork.Products.Should().NotBeNull();
        _unitOfWork.Categories.Should().NotBeNull();
        _unitOfWork.Reviews.Should().NotBeNull();
        _unitOfWork.Wishlists.Should().NotBeNull();
        _unitOfWork.AuditLogs.Should().NotBeNull();

        _unitOfWork.ProductGeneric.Should().NotBeNull();
        _unitOfWork.CategoryGeneric.Should().NotBeNull();
        _unitOfWork.UserGeneric.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldReturnAffectedRows()
    {
        // Arrange
        _context.Categories.Add(new Category
        {
            Id = 1,
            Name = "Electronics"
        });

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistData()
    {
        // Arrange
        _context.Categories.Add(new Category
        {
            Id = 2,
            Name = "Books"
        });

        // Act
        await _unitOfWork.SaveChangesAsync();

        // Assert
        _context.Categories.Should().HaveCount(1);

        _context.Categories.First().Name.Should().Be("Books");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldReturnZero_WhenNoChangesExist()
    {
        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void RepositoryProperties_ShouldReturnInjectedRepositories()
    {
        _unitOfWork.Products.Should().BeSameAs(_productRepositoryMock.Object);

        _unitOfWork.Categories.Should().BeSameAs(_categoryRepositoryMock.Object);

        _unitOfWork.Reviews.Should().BeSameAs(_reviewRepositoryMock.Object);

        _unitOfWork.Wishlists.Should().BeSameAs(_wishlistRepositoryMock.Object);

        _unitOfWork.AuditLogs.Should().BeSameAs(_auditRepositoryMock.Object);

        _unitOfWork.ProductGeneric.Should().BeSameAs(_productGenericMock.Object);

        _unitOfWork.CategoryGeneric.Should().BeSameAs(_categoryGenericMock.Object);

        _unitOfWork.UserGeneric.Should().BeSameAs(_userGenericMock.Object);
    }

}