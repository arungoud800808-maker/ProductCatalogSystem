using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.Repositories.Generic;
using Xunit;

namespace ProductService.Tests.Repositories;

public class GenericRepositoryTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly GenericRepository<Product> _repository;

    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ProductDbContext(options);

        SeedDatabase();

        _repository = new GenericRepository<Product>(_context);
    }

    private void SeedDatabase()
    {
        _context.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 50000,
                Stock = 10
            },
            new Product
            {
                Id = 2,
                Name = "Mouse",
                Price = 500,
                Stock = 50
            },
            new Product
            {
                Id = 3,
                Name = "Keyboard",
                Price = 1000,
                Stock = 25
            });

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var result = await _repository.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(100);

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        var result = await _repository.FindAsync(x => x.Price > 1000);

        result.Should().HaveCount(1);

        result.First().Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        var product = new Product
        {
            Id = 4,
            Name = "Monitor",
            Price = 12000,
            Stock = 8
        };

        await _repository.AddAsync(product);

        await _context.SaveChangesAsync();

        _context.Products.Count().Should().Be(4);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        var product = await _repository.GetByIdAsync(1);

        product!.Price = 60000;

        await _repository.UpdateAsync(product);

        await _context.SaveChangesAsync();

        _context.Products.Find(1)!.Price.Should().Be(60000);
    }


    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteEntity()
    {
        await _repository.DeleteAsync(1);

        var product = await _context.Products.IgnoreQueryFilters()
            .FirstAsync(x => x.Id == 1);

        product.IsDeleted.Should().BeTrue();
        product.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenEntityExists()
    {
        var result = await _repository.ExistsAsync(1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        var result = await _repository.ExistsAsync(100);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        var result = await _repository.CountAsync();

        result.Should().Be(3);
    }
}