using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductService.Controllers;
using ProductService.DTOs;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests.Controllers;

public class CategoryControllerTests
{
    private readonly Mock<ICategoryService> _serviceMock;
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _serviceMock = new Mock<ICategoryService>();

        _controller = new CategoryController(
            _serviceMock.Object);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOk()
    {
        // Arrange

        var categories = new List<CategoryDto>
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

        _serviceMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(categories);

        // Act

        var result = await _controller.GetCategories();

        // Assert

        var okResult = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var value = okResult.Value.Should()
            .BeAssignableTo<IEnumerable<CategoryDto>>()
            .Subject;

        value.Should().HaveCount(2);

        value.First().Name.Should().Be("Electronics");

        _serviceMock.Verify(
            x => x.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetCategory_WhenCategoryExists_ShouldReturnOk()
    {
        // Arrange

        int categoryId = 1;

        var category = new CategoryDto
        {
            Id = categoryId,
            Name = "Electronics"
        };

        _serviceMock
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        // Act

        var result = await _controller.GetCategory(categoryId);

        // Assert

        var okResult = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var value = okResult.Value.Should()
            .BeOfType<CategoryDto>()
            .Subject;

        value.Id.Should().Be(categoryId);
        value.Name.Should().Be("Electronics");

        _serviceMock.Verify(
            x => x.GetByIdAsync(categoryId),
            Times.Once);
    }

    [Fact]
    public async Task GetCategory_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange

        int categoryId = 100;

        _serviceMock
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync((CategoryDto?)null);

        // Act

        var result = await _controller.GetCategory(categoryId);

        // Assert

        var notFound = result.Result.Should()
            .BeOfType<NotFoundObjectResult>()
            .Subject;

        notFound.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _serviceMock.Verify(
            x => x.GetByIdAsync(categoryId),
            Times.Once);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        // Arrange

        var dto = new CreateCategoryDto
        {
            Name = "Electronics"
        };

        var category = new CategoryDto
        {
            Id = 1,
            Name = "Electronics"
        };

        _serviceMock
            .Setup(x => x.CreateAsync(dto))
            .ReturnsAsync(category);

        // Act

        var result = await _controller.Create(dto);

        // Assert

        var created = result.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        created.ActionName.Should().Be(nameof(CategoryController.GetCategory));

        var value = created.Value.Should()
            .BeOfType<CategoryDto>()
            .Subject;

        value.Id.Should().Be(1);
        value.Name.Should().Be("Electronics");

        _serviceMock.Verify(
            x => x.CreateAsync(dto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenCategoryExists_ShouldReturnOk()
    {
        // Arrange

        int categoryId = 1;

        var dto = new UpdateCategoryDto
        {
            Name = "Updated Category"
        };

        var updatedCategory = new CategoryDto
        {
            Id = categoryId,
            Name = "Updated Category"
        };

        _serviceMock
            .Setup(x => x.UpdateAsync(categoryId, dto))
            .ReturnsAsync(updatedCategory);

        // Act

        var result = await _controller.Update(categoryId, dto);

        // Assert

        var okResult = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var value = okResult.Value.Should()
            .BeOfType<CategoryDto>()
            .Subject;

        value.Name.Should().Be("Updated Category");

        _serviceMock.Verify(
            x => x.UpdateAsync(categoryId, dto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange

        int categoryId = 10;

        var dto = new UpdateCategoryDto
        {
            Name = "Updated Category"
        };

        _serviceMock
            .Setup(x => x.UpdateAsync(categoryId, dto))
            .ReturnsAsync((CategoryDto?)null);

        // Act

        var result = await _controller.Update(categoryId, dto);

        // Assert

        result.Result.Should()
            .BeOfType<NotFoundObjectResult>();

        _serviceMock.Verify(
            x => x.UpdateAsync(categoryId, dto),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenCategoryExists_ShouldReturnNoContent()
    {
        // Arrange

        int categoryId = 1;

        var category = new CategoryDto
        {
            Id = categoryId,
            Name = "Electronics"
        };

        _serviceMock
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        _serviceMock
            .Setup(x => x.DeleteAsync(categoryId))
            .Returns(Task.CompletedTask);

        // Act

        var result = await _controller.Delete(categoryId);

        // Assert

        result.Should()
            .BeOfType<NoContentResult>();

        _serviceMock.Verify(
            x => x.GetByIdAsync(categoryId),
            Times.Once);

        _serviceMock.Verify(
            x => x.DeleteAsync(categoryId),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange

        int categoryId = 100;

        _serviceMock
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync((CategoryDto?)null);

        // Act

        var result = await _controller.Delete(categoryId);

        // Assert

        result.Should()
            .BeOfType<NotFoundObjectResult>();

        _serviceMock.Verify(
            x => x.GetByIdAsync(categoryId),
            Times.Once);

        _serviceMock.Verify(
            x => x.DeleteAsync(It.IsAny<int>()),
            Times.Never);
    }
}