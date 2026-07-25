using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.Repositories;
using Xunit;

namespace ProductService.Tests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ProductDbContext(options);

        SeedDatabase();

        _repository = new ProductRepository(_context);
    }

    private void SeedDatabase()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        _context.Categories.Add(category);

        _context.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 50000,
                Stock = 20,
                CategoryId = 1
            },
            new Product
            {
                Id = 2,
                Name = "Mouse",
                Price = 500,
                Stock = 100,
                CategoryId = 1
            },
            new Product
            {
                Id = 3,
                Name = "Keyboard",
                Price = 1200,
                Stock = 5,
                CategoryId = 1
            });

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    [Fact]
    public async Task FilterProductsAsync_ShouldReturnProductsWithinPriceRange()
    {
        // Act

        var result = await _repository.FilterProductsAsync(
            1000,
            60000);

        // Assert

        result.Should().HaveCount(2);

        result.Should().OnlyContain(p =>
            p.Price >= 1000 &&
            p.Price <= 60000);
    }
    [Fact]
    public async Task SortProductsAsync_ByPrice_ShouldReturnSortedProducts()
    {
        var result = (await _repository.SortProductsAsync("price")).ToList();

        result[0].Price.Should().Be(500);
        result[1].Price.Should().Be(1200);
        result[2].Price.Should().Be(50000);
    }
    [Fact]
    public async Task SortProductsAsync_ByName_ShouldReturnSortedProducts()
    {
        var result = (await _repository.SortProductsAsync("name")).ToList();

        result[0].Name.Should().Be("Keyboard");
        result[1].Name.Should().Be("Laptop");
        result[2].Name.Should().Be("Mouse");
    }
    [Fact]
    public async Task SortProductsAsync_ByStock_ShouldReturnSortedProducts()
    {
        var result = (await _repository.SortProductsAsync("stock")).ToList();

        result[0].Stock.Should().Be(5);
        result[1].Stock.Should().Be(20);
        result[2].Stock.Should().Be(100);
    }
    [Fact]
    public async Task SortProductsAsync_InvalidSort_ShouldSortById()
    {
        var result = (await _repository.SortProductsAsync("xyz")).ToList();

        result[0].Id.Should().Be(1);
        result[1].Id.Should().Be(2);
        result[2].Id.Should().Be(3);
    }
    [Fact]
    public async Task GetPagedProductsAsync_ShouldReturnRequestedPage()
    {
        var result = (await _repository.GetPagedProductsAsync(2, 1)).ToList();

        result.Should().HaveCount(1);

        result[0].Id.Should().Be(2);
    }
    [Fact]
    public async Task SearchProductsAsync_ShouldReturnMatchingProducts()
    {
        var result = (await _repository.SearchProductsAsync("Lap")).ToList();

        result.Should().HaveCount(1);

        result[0].Name.Should().Be("Laptop");
    }
    [Fact]
    public async Task GetProductsByCategoryAsync_ShouldReturnCategoryProducts()
    {
        var result =
            (await _repository.GetProductsByCategoryAsync(1)).ToList();

        result.Should().HaveCount(3);
    }
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);

        result.Should().Contain(p => p.Name == "Laptop");
        result.Should().Contain(p => p.Name == "Mouse");
        result.Should().Contain(p => p.Name == "Keyboard");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
    {
        // Act
        var product = await _repository.GetByIdAsync(1);

        // Assert
        product.Should().NotBeNull();
        product!.Id.Should().Be(1);
        product.Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Act
        var product = await _repository.GetByIdAsync(999);

        // Assert
        product.Should().BeNull();
    }

    [Fact]
    public async Task BulkInsertAsync_ShouldInsertProducts()
    {
        // Arrange
        var products = new List<Product>
    {
        new()
        {
            Id = 4,
            Name = "Monitor",
            Price = 12000,
            Stock = 15,
            CategoryId = 1
        },
        new()
        {
            Id = 5,
            Name = "Speaker",
            Price = 3000,
            Stock = 20,
            CategoryId = 1
        }
    };

        // Act
        await _repository.BulkInsertAsync(products);

        await _context.SaveChangesAsync();

        // Assert
        _context.Products.Count().Should().Be(5);
    }

    [Fact]
    public async Task BulkUpdateAsync_ShouldUpdateProducts()
    {
        // Arrange
        var products = _context.Products.ToList();

        products[0].Price = 55000;
        products[1].Stock = 50;

        // Act
        await _repository.BulkUpdateAsync(products);

        await _context.SaveChangesAsync();

        // Assert
        _context.Products.Find(1)!.Price.Should().Be(55000);

        _context.Products.Find(2)!.Stock.Should().Be(50);
    }

    [Fact]
    public async Task BulkDeleteAsync_ShouldDeleteProducts()
    {
        // Arrange
        var ids = new List<int> { 1, 2 };

        // Act
        await _repository.BulkDeleteAsync(ids);

        await _context.SaveChangesAsync();

        // Assert
        _context.Products.Count().Should().Be(1);

        _context.Products.First().Id.Should().Be(3);
    }

    [Fact]
    public async Task ImportProductsAsync_ShouldImportProducts()
    {
        // Arrange
        var products = new List<Product>
    {
        new()
        {
            Id = 10,
            Name = "Printer",
            Price = 10000,
            Stock = 8,
            CategoryId = 1
        },
        new()
        {
            Id = 11,
            Name = "Scanner",
            Price = 7000,
            Stock = 4,
            CategoryId = 1
        }
    };

        // Act
        await _repository.ImportProductsAsync(products);

        await _context.SaveChangesAsync();

        // Assert
        _context.Products.Count().Should().Be(5);

        _context.Products.Any(p => p.Name == "Printer").Should().BeTrue();

        _context.Products.Any(p => p.Name == "Scanner").Should().BeTrue();
    }
    [Fact]
    public async Task UploadImageAsync_ShouldSaveFile_AndReturnRelativePath()
    {
        // Arrange
        var fileName = "test.png";

        var content = "Dummy Image Content";

        var bytes = System.Text.Encoding.UTF8.GetBytes(content);

        var stream = new MemoryStream(bytes);

        IFormFile file = new FormFile(
            stream,
            0,
            bytes.Length,
            "image",
            fileName);

        // Act
        var path = await _repository.UploadImageAsync(file);

        // Assert
        path.Should().StartWith("images/");

        var physicalPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            path.Replace("/", Path.DirectorySeparatorChar.ToString()));

        File.Exists(physicalPath).Should().BeTrue();

        // Cleanup
        if (File.Exists(physicalPath))
            File.Delete(physicalPath);
    }
    [Fact]
    public async Task UploadImageAsync_ShouldThrowException_WhenFileIsNull()
    {
        // Arrange
        IFormFile? file = null;

        // Act
        Func<Task> action = async () =>
            await _repository.UploadImageAsync(file!);

        // Assert
        await action.Should()
            .ThrowAsync<Exception>()
            .WithMessage("No file selected.");
    }
    [Fact]
    public async Task GetDashboardAsync_ShouldReturnDashboardStatistics()
    {
        // Arrange

        _context.Users.Add(new User
        {
            Id = 1,
            FullName = "Admin User",
            Email = "admin@test.com",
            PasswordHash = "DummyPasswordHash",
            Role = "Admin"
        });

        _context.Reviews.Add(new Review
        {
            Id = 1,
            ProductId = 1,
            Rating = 5
        });

        _context.Wishlists.Add(new Wishlist
        {
            Id = 1,
            UserId = 1,
            ProductId = 1
        });

        await _context.SaveChangesAsync();

        // Act

        var result = await _repository.GetDashboardAsync();

        // Assert

        result.Should().NotBeNull();

        result.TotalProducts.Should().Be(3);

        result.TotalCategories.Should().Be(1);

        result.TotalUsers.Should().Be(1);

        result.TotalReviews.Should().Be(1);

        result.TotalWishlists.Should().Be(1);

        result.LowStockProducts.Should().Be(1);
    }

}