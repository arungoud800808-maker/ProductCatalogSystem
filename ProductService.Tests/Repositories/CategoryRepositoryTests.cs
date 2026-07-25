using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.Repositories;
using Xunit;

namespace ProductService.Tests.Repositories;

public class CategoryRepositoryTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ProductDbContext(options);

        SeedDatabase();

        _repository = new CategoryRepository(_context);
    }

    private void SeedDatabase()
    {
        _context.Categories.AddRange(
            new Category
            {
                Id = 1,
                Name = "Electronics"
            },
            new Category
            {
                Id = 2,
                Name = "Furniture"
            });

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Act

        var result = (await _repository.GetAllAsync()).ToList();

        // Assert

        result.Should().HaveCount(2);

        result.Should().Contain(c => c.Name == "Electronics");
        result.Should().Contain(c => c.Name == "Furniture");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenExists()
    {
        // Act

        var category = await _repository.GetByIdAsync(1);

        // Assert

        category.Should().NotBeNull();

        category!.Id.Should().Be(1);

        category.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        // Act

        var category = await _repository.GetByIdAsync(100);

        // Assert

        category.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddCategory()
    {
        // Arrange

        var category = new Category
        {
            Id = 3,
            Name = "Books"
        };

        // Act

        var result = await _repository.AddAsync(category);

        // Assert

        result.Should().NotBeNull();

        result.Name.Should().Be("Books");

        _context.Categories.Count().Should().Be(3);

        _context.Categories.Any(c => c.Name == "Books")
            .Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory()
    {
        // Arrange

        var category = new Category
        {
            Id = 1,
            Name = "Updated Electronics"
        };

        // Act

        var result = await _repository.UpdateAsync(category);

        // Assert

        result.Should().NotBeNull();

        result!.Name.Should().Be("Updated Electronics");

        _context.Categories.Find(1)!.Name
            .Should().Be("Updated Electronics");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        // Arrange

        var category = new Category
        {
            Id = 100,
            Name = "Invalid"
        };

        // Act

        var result = await _repository.UpdateAsync(category);

        // Assert

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory()
    {
        // Arrange

        var category = await _repository.GetByIdAsync(1);

        // Act

        await _repository.DeleteAsync(category!);

        // Assert

        _context.Categories.Count().Should().Be(1);

        _context.Categories.Any(c => c.Id == 1)
            .Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyCollection_WhenDatabaseIsEmpty()
    {
        // Arrange

        _context.Categories.RemoveRange(_context.Categories);

        await _context.SaveChangesAsync();

        // Act

        var result = await _repository.GetAllAsync();

        // Assert

        result.Should().BeEmpty();
    }
}