using AutoMapper;
using FluentAssertions;
using Moq;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();

        _service = new CategoryService(
            _repositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnCategories()
    {
        // Arrange

        var categories = new List<Category>
        {
            new()
            {
                Id = 1,
                Name = "Electronics"
            },
            new()
            {
                Id = 2,
                Name = "Furniture"
            }
        };

        var categoryDtos = new List<CategoryDto>
        {
            new()
            {
                Id = 1,
                Name = "Electronics"
            },
            new()
            {
                Id = 2,
                Name = "Furniture"
            }
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(categories);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<CategoryDto>>(categories))
            .Returns(categoryDtos);

        // Act

        var result = await _service.GetAllAsync();

        // Assert

        result.Should().HaveCount(2);

        result.First().Name.Should().Be("Electronics");

        _repositoryMock.Verify(
            x => x.GetAllAsync(),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<IEnumerable<CategoryDto>>(categories),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ShouldReturnCategory()
    {
        // Arrange

        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        var dto = new CategoryDto
        {
            Id = 1,
            Name = "Electronics"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(category);

        _mapperMock
            .Setup(x => x.Map<CategoryDto>(category))
            .Returns(dto);

        // Act

        var result = await _service.GetByIdAsync(1);

        // Assert

        result.Should().NotBeNull();

        result!.Name.Should().Be("Electronics");

        _repositoryMock.Verify(
            x => x.GetByIdAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        // Arrange

        _repositoryMock
            .Setup(x => x.GetByIdAsync(100))
            .ReturnsAsync((Category?)null);

        // Act

        var result = await _service.GetByIdAsync(100);

        // Assert

        result.Should().BeNull();

        _mapperMock.Verify(
            x => x.Map<CategoryDto>(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCategory()
    {
        // Arrange

        var createDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };

        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        var categoryDto = new CategoryDto
        {
            Id = 1,
            Name = "Electronics"
        };

        _mapperMock
            .Setup(x => x.Map<Category>(createDto))
            .Returns(category);

        _repositoryMock
    .Setup(x => x.AddAsync(category))
    .ReturnsAsync(category);

        _mapperMock
            .Setup(x => x.Map<CategoryDto>(category))
            .Returns(categoryDto);

        // Act

        var result = await _service.CreateAsync(createDto);

        // Assert

        result.Should().NotBeNull();

        result.Name.Should().Be("Electronics");

        _repositoryMock.Verify(
            x => x.AddAsync(category),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryExists_ShouldReturnUpdatedCategory()
    {
        // Arrange

        var category = new Category
        {
            Id = 1,
            Name = "Old"
        };

        var dto = new UpdateCategoryDto
        {
            Name = "Updated"
        };

        var categoryDto = new CategoryDto
        {
            Id = 1,
            Name = "Updated"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(category);

        _repositoryMock
     .Setup(x => x.UpdateAsync(category))
     .ReturnsAsync(category);
        _mapperMock
            .Setup(x => x.Map<CategoryDto>(category))
            .Returns(categoryDto);

        // Act

        var result = await _service.UpdateAsync(1, dto);

        // Assert

        result.Should().NotBeNull();

        result!.Name.Should().Be("Updated");

        _repositoryMock.Verify(
            x => x.UpdateAsync(category),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        // Arrange

        var dto = new UpdateCategoryDto
        {
            Name = "Updated"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(100))
            .ReturnsAsync((Category?)null);

        // Act

        var result = await _service.UpdateAsync(100, dto);

        // Assert

        result.Should().BeNull();

        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryExists_ShouldDeleteCategory()
    {
        // Arrange

        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(x => x.DeleteAsync(category))
            .Returns(Task.CompletedTask);

        // Act

        await _service.DeleteAsync(1);

        // Assert

        _repositoryMock.Verify(
            x => x.DeleteAsync(category),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryDoesNotExist_ShouldNotDelete()
    {
        // Arrange

        _repositoryMock
            .Setup(x => x.GetByIdAsync(100))
            .ReturnsAsync((Category?)null);

        // Act

        await _service.DeleteAsync(100);

        // Assert

        _repositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<Category>()),
            Times.Never);
    }
}