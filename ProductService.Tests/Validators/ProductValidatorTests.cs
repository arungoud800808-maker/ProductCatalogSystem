using FluentValidation.TestHelper;
using ProductService.DTOs;
using ProductService.Validators;
using Xunit;

namespace ProductService.Tests.Validators;

public class ProductValidatorTests
{
    private readonly ProductValidator _validator;

    public ProductValidatorTests()
    {
        _validator = new ProductValidator();
    }

    [Fact]
    public void Should_NotHaveValidationError_When_ModelIsValid()
    {
        // Arrange
        var model = new CreateProductDto
        {
            Name = "Laptop",
            Description = "Gaming Laptop",
            Price = 50000,
            Stock = 10,
            CategoryId = 1,
            ImageUrl = "laptop.jpg"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveValidationError_When_NameIsEmpty()
    {
        // Arrange
        var model = new CreateProductDto
        {
            Name = "",
            Price = 50000,
            Stock = 10
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveValidationError_When_NameIsNull()
    {
        // Arrange
        var model = new CreateProductDto
        {
            Name = null!,
            Price = 50000,
            Stock = 10
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveValidationError_When_NameExceeds100Characters()
    {
        // Arrange
        var model = new CreateProductDto
        {
            Name = new string('A', 101),
            Price = 50000,
            Stock = 10
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveValidationError_When_PriceIsZero()
    {
        // Arrange
        var model = new CreateProductDto
        {
            Name = "Laptop",
            Price = 0,
            Stock = 10
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_HaveValidationError_When_PriceIsNegative()
    {
        // Arrange
        var model = new CreateProductDto
        {
            Name = "Laptop",
            Price = -100,
            Stock = 10
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_NotHaveValidationError_When_PriceIsGreaterThanZero()
    {
        // Arrange
        var model = new CreateProductDto
        {
            Name = "Laptop",
            Price = 1,
            Stock = 10
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_HaveValidationError_When_StockIsNegative()
    {
        // Arrange
        var model = new CreateProductDto
        {
            Name = "Laptop",
            Price = 50000,
            Stock = -1
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Stock);
    }

    [Fact]
    public void Should_NotHaveValidationError_When_StockIsZero()
    {
        // Arrange
        var model = new CreateProductDto
        {
            Name = "Laptop",
            Price = 50000,
            Stock = 0
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Stock);
    }

    [Fact]
    public void Should_NotHaveValidationError_When_StockIsPositive()
    {
        // Arrange
        var model = new CreateProductDto
        {
            Name = "Laptop",
            Price = 50000,
            Stock = 25
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Stock);
    }
}