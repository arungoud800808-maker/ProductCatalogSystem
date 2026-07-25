using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductService.Application.Commands.Products.CreateProduct;
using ProductService.Application.Commands.Products.DeleteProduct;
using ProductService.Application.Commands.Products.UpdateProduct;
using ProductService.Application.Queries.Products.GetProductById;
using ProductService.Application.Queries.Products.GetProducts;
using ProductService.Controllers;
using ProductService.DTOs;
using ProductService.Services;
using ProductService.Wrappers;
using Xunit;

namespace ProductService.Tests.Controllers;

public class ProductControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IProductService> _serviceMock;

    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();

        _serviceMock = new Mock<IProductService>();

        _controller = new ProductController(
            _mediatorMock.Object,
            _serviceMock.Object);
    }
    [Fact]
    public async Task GetAllProducts_ShouldReturnOkResult()
    {
        // Arrange

        var products = new List<ProductDto>
    {
        new ProductDto
        {
            Id = 1,
            Name = "Laptop"
        },
        new ProductDto
        {
            Id = 2,
            Name = "Mouse"
        }
    };

        _serviceMock
            .Setup(x => x.GetProductsAsync())
            .ReturnsAsync(products);

        // Act

        var result = await _controller.GetAllProducts();

        // Assert

        var okResult = result.Should().BeOfType<OkObjectResult>()
            .Subject;

        var value = okResult.Value.Should()
            .BeAssignableTo<IEnumerable<ProductDto>>()
            .Subject;

        value.Should().HaveCount(2);

        value.First().Name.Should().Be("Laptop");

        _serviceMock.Verify(
            x => x.GetProductsAsync(),
            Times.Once);
    }
    [Fact]
    public async Task GetProduct_WhenProductExists_ShouldReturnOk()
    {
        // Arrange

        int productId = 1;

        var product = new ProductDto
        {
            Id = productId,
            Name = "Laptop",
            Price = 65000
        };

        _mediatorMock
            .Setup(x => x.Send(
                It.IsAny<GetProductByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act

        var result = await _controller.GetProduct(productId);

        // Assert

        var okResult = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<ApiResponse<ProductDto>>()
            .Subject;

        response.Success.Should().BeTrue();

        response.Data.Should().NotBeNull();

        response.Data!.Id.Should().Be(productId);

        response.Data.Name.Should().Be("Laptop");

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<GetProductByIdQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task GetProduct_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange

        int productId = 100;

        _mediatorMock
            .Setup(x => x.Send(
                It.IsAny<GetProductByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);

        // Act

        var result = await _controller.GetProduct(productId);

        // Assert

        var notFoundResult = result.Result.Should()
            .BeOfType<NotFoundObjectResult>()
            .Subject;

        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<GetProductByIdQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task GetProducts_ShouldReturnOk()
    {
        // Arrange

        var products = new List<ProductDto>
    {
        new ProductDto
        {
            Id = 1,
            Name = "Laptop"
        },
        new ProductDto
        {
            Id = 2,
            Name = "Mouse"
        }
    };

        _mediatorMock
            .Setup(x => x.Send(
                It.IsAny<GetProductsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act

        var result = await _controller.GetProducts(
            null,
            null,
            1,
            10);

        // Assert

        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<ApiResponse<IEnumerable<ProductDto>>>()
            .Subject;

        response.Success.Should().BeTrue();

        response.Data.Should().HaveCount(2);

        response.Data!.First().Name.Should().Be("Laptop");

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<GetProductsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Create_WhenModelIsValid_ShouldReturnCreatedAtAction()
    {
        // Arrange

        var dto = new CreateProductDto
        {
            Name = "Laptop",
            Description = "Gaming Laptop",
            Price = 65000,
            Stock = 10
        };

        var createdProduct = new ProductDto
        {
            Id = 1,
            Name = "Laptop",
            Description = "Gaming Laptop",
            Price = 65000,
            Stock = 10
        };

        _mediatorMock
            .Setup(x => x.Send(
                It.IsAny<CreateProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act

        var result = await _controller.Create(dto);

        // Assert

        var createdResult = result.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        createdResult.ActionName.Should().Be(nameof(ProductController.GetProduct));

        var response = createdResult.Value.Should()
            .BeOfType<ApiResponse<ProductDto>>()
            .Subject;

        response.Success.Should().BeTrue();

        response.Data.Should().NotBeNull();

        response.Data!.Id.Should().Be(1);

        response.Data.Name.Should().Be("Laptop");

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<CreateProductCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Create_WhenModelStateIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange

        var dto = new CreateProductDto();

        _controller.ModelState.AddModelError(
            "Name",
            "Name is required.");

        // Act

        var result = await _controller.Create(dto);

        // Assert

        var badRequest = result.Result.Should()
            .BeOfType<BadRequestObjectResult>()
            .Subject;

        badRequest.StatusCode.Should()
            .Be(StatusCodes.Status400BadRequest);

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<CreateProductCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task Update_WhenProductExists_ShouldReturnOk()
    {
        // Arrange

        int productId = 1;

        var dto = new UpdateProductDto
        {
            Name = "Updated Laptop",
            Description = "Updated Description",
            Price = 70000,
            Stock = 20,
            ImageUrl = "laptop.jpg",
            CategoryId = 1
        };

        var updatedProduct = new ProductDto
        {
            Id = productId,
            Name = "Updated Laptop",
            Description = "Updated Description",
            Price = 70000,
            Stock = 20,
            ImageUrl = "laptop.jpg",
            CategoryId = 1
        };

        _mediatorMock
            .Setup(x => x.Send(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProduct);

        // Act

        var result = await _controller.Update(productId, dto);

        // Assert

        var okResult = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<ApiResponse<ProductDto>>()
            .Subject;

        response.Success.Should().BeTrue();

        response.Data.Should().NotBeNull();

        response.Data!.Id.Should().Be(productId);
        response.Data.Name.Should().Be("Updated Laptop");
        response.Data.Price.Should().Be(70000);

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Update_WhenModelStateIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange

        var dto = new UpdateProductDto();

        _controller.ModelState.AddModelError(
            "Name",
            "Name is required.");

        // Act

        var result = await _controller.Update(1, dto);

        // Assert

        var badRequest = result.Result.Should()
            .BeOfType<BadRequestObjectResult>()
            .Subject;

        badRequest.StatusCode.Should()
            .Be(StatusCodes.Status400BadRequest);

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task Update_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange

        int productId = 100;

        var dto = new UpdateProductDto
        {
            Name = "Laptop",
            Description = "Gaming Laptop",
            Price = 65000,
            Stock = 10,
            ImageUrl = "laptop.jpg",
            CategoryId = 1
        };

        _mediatorMock
            .Setup(x => x.Send(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);

        // Act

        var result = await _controller.Update(productId, dto);

        // Assert

        var notFound = result.Result.Should()
            .BeOfType<NotFoundObjectResult>()
            .Subject;

        notFound.StatusCode.Should()
            .Be(StatusCodes.Status404NotFound);

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Delete_WhenProductExists_ShouldReturnOk()
    {
        // Arrange

        int productId = 1;

        _mediatorMock
            .Setup(x => x.Send(
                It.IsAny<DeleteProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act

        var result = await _controller.Delete(productId);

        // Assert

        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<ApiResponse<string>>()
            .Subject;

        response.Success.Should().BeTrue();

        response.Message.Should().Be("Product deleted successfully.");

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<DeleteProductCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Delete_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange

        int productId = 100;

        _mediatorMock
            .Setup(x => x.Send(
                It.IsAny<DeleteProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act

        var result = await _controller.Delete(productId);

        // Assert

        var notFound = result.Should()
            .BeOfType<NotFoundObjectResult>()
            .Subject;

        notFound.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<DeleteProductCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Search_ShouldReturnMatchingProducts()
    {
        // Arrange

        var keyword = "Laptop";

        var products = new List<ProductDto>
    {
        new ProductDto
        {
            Id = 1,
            Name = "Gaming Laptop",
            Price = 65000
        },
        new ProductDto
        {
            Id = 2,
            Name = "Office Laptop",
            Price = 45000
        }
    };

        _serviceMock
            .Setup(x => x.SearchProductsAsync(keyword))
            .ReturnsAsync(products);

        // Act

        var result = await _controller.Search(keyword);

        // Assert

        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<ApiResponse<IEnumerable<ProductDto>>>()
            .Subject;

        response.Success.Should().BeTrue();

        response.Message.Should().Be("Search completed successfully.");

        response.Data.Should().HaveCount(2);

        response.Data!.First().Name.Should().Be("Gaming Laptop");

        _serviceMock.Verify(
            x => x.SearchProductsAsync(keyword),
            Times.Once);
    }
    [Fact]
    public async Task GetPagedProducts_ShouldReturnOk()
    {
        // Arrange

        int pageNumber = 1;
        int pageSize = 5;

        var products = new List<ProductDto>
    {
        new ProductDto
        {
            Id = 1,
            Name = "Laptop"
        },
        new ProductDto
        {
            Id = 2,
            Name = "Mouse"
        }
    };

        _serviceMock
            .Setup(x => x.GetPagedProductsAsync(pageNumber, pageSize))
            .ReturnsAsync(products);

        // Act

        var result = await _controller.GetPagedProducts(pageNumber, pageSize);

        // Assert

        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<ApiResponse<IEnumerable<ProductDto>>>()
            .Subject;

        response.Success.Should().BeTrue();

        response.Message.Should().Be("Products retrieved successfully.");

        response.Data.Should().HaveCount(2);

        response.Data!.First().Name.Should().Be("Laptop");

        _serviceMock.Verify(
            x => x.GetPagedProductsAsync(pageNumber, pageSize),
            Times.Once);
    }
    [Fact]
    public async Task GetProductsByCategory_WhenProductsExist_ShouldReturnOk()
    {
        // Arrange

        int categoryId = 1;

        var products = new List<ProductDto>
    {
        new ProductDto
        {
            Id = 1,
            Name = "Laptop"
        },
        new ProductDto
        {
            Id = 2,
            Name = "Mouse"
        }
    };

        _serviceMock
            .Setup(x => x.GetProductsByCategoryAsync(categoryId))
            .ReturnsAsync(products);

        // Act

        var result = await _controller.GetProductsByCategory(categoryId);

        // Assert

        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = okResult.Value.Should()
            .BeOfType<ApiResponse<IEnumerable<ProductDto>>>()
            .Subject;

        response.Success.Should().BeTrue();

        response.Message.Should().Be("Category products retrieved successfully.");

        response.Data.Should().HaveCount(2);

        _serviceMock.Verify(
            x => x.GetProductsByCategoryAsync(categoryId),
            Times.Once);
    }
    [Fact]
    public async Task GetProductsByCategory_WhenNoProductsExist_ShouldReturnNotFound()
    {
        // Arrange

        int categoryId = 100;

        _serviceMock
            .Setup(x => x.GetProductsByCategoryAsync(categoryId))
            .ReturnsAsync(new List<ProductDto>());

        // Act

        var result = await _controller.GetProductsByCategory(categoryId);

        // Assert

        var notFound = result.Should()
            .BeOfType<NotFoundObjectResult>()
            .Subject;

        var response = notFound.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Success.Should().BeFalse();

        response.Message.Should().Be("No products found for this category.");

        _serviceMock.Verify(
            x => x.GetProductsByCategoryAsync(categoryId),
            Times.Once);
    }
    [Fact]
    public async Task UploadImage_ShouldReturnImageUrl()
    {
        // Arrange

        var fileMock = new Mock<IFormFile>();

        var imagePath = "images/products/laptop.jpg";

        _serviceMock
            .Setup(x => x.UploadImageAsync(fileMock.Object))
            .ReturnsAsync(imagePath);

        // Act

        var result = await _controller.UploadImage(fileMock.Object);

        // Assert

        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        // Verify anonymous object property
        var imageUrl = okResult.Value!
            .GetType()
            .GetProperty("ImageUrl")!
            .GetValue(okResult.Value);

        imageUrl.Should().Be(imagePath);

        _serviceMock.Verify(
            x => x.UploadImageAsync(fileMock.Object),
            Times.Once);
    }
    [Fact]
    public async Task BulkInsert_ShouldReturnOk()
    {
        var dto = new BulkProductDto
        {
            Products = new List<CreateProductDto>
        {
            new()
            {
                Name="Laptop",
                Description="Gaming",
                Price=65000,
                Stock=10,
                CategoryId=1
            }
        }
        };

        var result = await _controller.BulkInsert(dto);

        var ok = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var response = ok.Value.Should()
            .BeOfType<ApiResponse<object>>()
            .Subject;

        response.Success.Should().BeTrue();

        _serviceMock.Verify(
            x => x.BulkInsertAsync(dto),
            Times.Once);
    }
    [Fact]
    public async Task BulkInsert_WhenListEmpty_ShouldReturnBadRequest()
    {
        var dto = new BulkProductDto
        {
            Products = new List<CreateProductDto>()
        };

        var result = await _controller.BulkInsert(dto);

        result.Should().BeOfType<BadRequestObjectResult>();

        _serviceMock.Verify(
            x => x.BulkInsertAsync(It.IsAny<BulkProductDto>()),
            Times.Never);
    }
    [Fact]
    public async Task BulkUpdate_ShouldReturnOk()
    {
        var dto = new UpdateBulkProductDto
        {
            Products = new List<UpdateProductDtoWithId>
        {
            new()
            {
                Id=1,
                Name="Laptop",
                Description="Gaming",
                Price=65000,
                Stock=10,
                CategoryId=1
            }
        }
        };

        var result = await _controller.BulkUpdate(dto);

        result.Should().BeOfType<OkObjectResult>();

        _serviceMock.Verify(
            x => x.BulkUpdateAsync(dto),
            Times.Once);
    }
    [Fact]
    public async Task BulkUpdate_WhenListEmpty_ShouldReturnBadRequest()
    {
        var dto = new UpdateBulkProductDto
        {
            Products = new List<UpdateProductDtoWithId>()
        };

        var result = await _controller.BulkUpdate(dto);

        result.Should().BeOfType<BadRequestObjectResult>();

        _serviceMock.Verify(
            x => x.BulkUpdateAsync(It.IsAny<UpdateBulkProductDto>()),
            Times.Never);
    }
    [Fact]
    public async Task BulkDelete_ShouldReturnOk()
    {
        var dto = new BulkDeleteDto
        {
            ProductIds = new List<int> { 1, 2, 3 }
        };

        var result = await _controller.BulkDelete(dto);

        result.Should().BeOfType<OkObjectResult>();

        _serviceMock.Verify(
            x => x.BulkDeleteAsync(dto),
            Times.Once);
    }
    [Fact]
    public async Task BulkDelete_WhenListEmpty_ShouldReturnBadRequest()
    {
        var dto = new BulkDeleteDto
        {
            ProductIds = new List<int>()
        };

        var result = await _controller.BulkDelete(dto);

        result.Should().BeOfType<BadRequestObjectResult>();

        _serviceMock.Verify(
            x => x.BulkDeleteAsync(It.IsAny<BulkDeleteDto>()),
            Times.Never);
    }
    [Fact]
    public async Task ExportProducts_ShouldReturnFile()
    {
        var bytes = new byte[] { 1, 2, 3 };

        _serviceMock
            .Setup(x => x.ExportProductsAsync())
            .ReturnsAsync(bytes);

        var result = await _controller.ExportProducts();

        var file = result.Should()
            .BeOfType<FileContentResult>()
            .Subject;

        file.FileDownloadName.Should().Be("Products.xlsx");

        file.ContentType.Should().Be(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        _serviceMock.Verify(
            x => x.ExportProductsAsync(),
            Times.Once);
    }
    [Fact]
    public async Task ImportProducts_ShouldReturnOk()
    {
        var file = new Mock<IFormFile>();

        var result = await _controller.ImportProducts(file.Object);

        result.Should().BeOfType<OkObjectResult>();

        _serviceMock.Verify(
            x => x.ImportProductsAsync(file.Object),
            Times.Once);
    }
    [Fact]
    public async Task Dashboard_ShouldReturnOk()
    {
        var dashboard = new DashboardDto
        {
            TotalProducts = 10,
            TotalCategories = 2,
            TotalUsers = 5,
            TotalStock = 100,
            TotalReviews = 20,
            TotalWishlists = 30,
            OutOfStockProducts = 1,
            LowStockProducts = 2,
            AveragePrice = 1000,
            HighestPrice = 5000,
            LowestPrice = 100
        };

        _serviceMock
            .Setup(x => x.GetDashboardAsync())
            .ReturnsAsync(dashboard);

        var result = await _controller.Dashboard();

        var ok = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        var value = ok.Value.Should()
            .BeOfType<DashboardDto>()
            .Subject;

        value.TotalProducts.Should().Be(10);

        _serviceMock.Verify(
            x => x.GetDashboardAsync(),
            Times.Once);
    }
    [Fact]
    public async Task Filter_ShouldReturnOk()
    {
        var products = new List<ProductDto>
    {
        new()
        {
            Id=1,
            Name="Laptop"
        }
    };

        _serviceMock
            .Setup(x => x.FilterProductsAsync(100, 500))
            .ReturnsAsync(products);

        var result = await _controller.Filter(100, 500);

        result.Should().BeOfType<OkObjectResult>();

        _serviceMock.Verify(
            x => x.FilterProductsAsync(100, 500),
            Times.Once);
    }
    [Fact]
    public async Task SortProducts_ShouldReturnOk()
    {
        var products = new List<ProductDto>
    {
        new()
        {
            Id=1,
            Name="Laptop"
        }
    };

        _serviceMock
            .Setup(x => x.SortProductsAsync("price"))
            .ReturnsAsync(products);

        var result = await _controller.SortProducts("price");

        result.Should().BeOfType<OkObjectResult>();

        _serviceMock.Verify(
            x => x.SortProductsAsync("price"),
            Times.Once);
    }
}