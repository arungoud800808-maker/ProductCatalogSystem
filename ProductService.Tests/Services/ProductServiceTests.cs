using AutoMapper;
using ClosedXML.Excel;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Repositories.UnitOfWork;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests.Services;

public class ProductServiceTests
{
    private readonly ProductDbContext _context;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<ProductService.Services.ProductService>> _loggerMock;

    private readonly ProductService.Services.ProductService _service;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _mapperMock = new Mock<IMapper>();

        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _loggerMock = new Mock<ILogger<ProductService.Services.ProductService>>();

        var options = new DbContextOptionsBuilder<ProductDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;


        _context = new ProductDbContext(options);


        _service = new ProductService.Services.ProductService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _memoryCache,
            _loggerMock.Object,
            _context);
    }
        [Fact]
    public async Task GetProductsAsync_Should_Return_Cached_Products_When_Cache_Exists()
    {
        // Arrange

        var expectedProducts = new List<ProductDto>
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

        _memoryCache.Set("PRODUCT_LIST", expectedProducts);

        // Act

        var result = await _service.GetProductsAsync();

        // Assert

        result.Should().NotBeNull();

        result.Count().Should().Be(2);

        result.First().Name.Should().Be("Laptop");
    }
    [Fact]
    public async Task GetProductsAsync_Should_Return_Products_From_Database_When_Cache_Is_Empty()
    {
        // Arrange

        var products = new List<ProductService.Models.Product>
    {
        new ProductService.Models.Product
        {
            Id = 1,
            Name = "Laptop"
        },
        new ProductService.Models.Product
        {
            Id = 2,
            Name = "Keyboard"
        }
    };

        var productDtos = new List<ProductDto>
    {
        new ProductDto
        {
            Id = 1,
            Name = "Laptop"
        },
        new ProductDto
        {
            Id = 2,
            Name = "Keyboard"
        }
    };

        var productRepositoryMock = new Mock<IProductRepository>();

        productRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);

        // Act

        var result = await _service.GetProductsAsync();

        // Assert

        result.Should().NotBeNull();

        result.Count().Should().Be(2);

        result.First().Name.Should().Be("Laptop");

        _unitOfWorkMock.Verify(x => x.Products, Times.Once);

        productRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);

        _mapperMock.Verify(x => x.Map<IEnumerable<ProductDto>>(products), Times.Once);
    }
    [Fact]
    public async Task GetProductAsync_Should_Return_Product_When_Product_Exists()
    {
        // Arrange

        int productId = 1;

        var product = new ProductService.Models.Product
        {
            Id = productId,
            Name = "Laptop",
            Price = 50000
        };

        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Laptop",
            Price = 50000
        };

        var productRepositoryMock = new Mock<IProductRepository>();

        productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(product))
            .Returns(productDto);

        // Act

        var result = await _service.GetProductAsync(productId);

        // Assert

        result.Should().NotBeNull();

        result!.Id.Should().Be(productId);

        result.Name.Should().Be("Laptop");

        result.Price.Should().Be(50000);

        productRepositoryMock.Verify(x => x.GetByIdAsync(productId), Times.Once);

        _mapperMock.Verify(x => x.Map<ProductDto>(product), Times.Once);
    }
    [Fact]
    public async Task GetProductAsync_Should_Return_Null_When_Product_Does_Not_Exist()
    {
        // Arrange

        int productId = 100;

        var productRepositoryMock = new Mock<IProductRepository>();

        productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((ProductService.Models.Product?)null);

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        // Act

        var result = await _service.GetProductAsync(productId);

        // Assert

        result.Should().BeNull();

        productRepositoryMock.Verify(x => x.GetByIdAsync(productId), Times.Once);

        _mapperMock.Verify(
            x => x.Map<ProductDto>(It.IsAny<ProductService.Models.Product>()),
            Times.Never);
    }
    [Fact]
    public async Task AddProductAsync_Should_Add_Product_And_Return_ProductDto()
    {
        // Arrange

        var createDto = new CreateProductDto
        {
            Name = "Laptop",
            Description = "Gaming Laptop",
            Price = 65000,
            Stock = 10
        };

        var product = new ProductService.Models.Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "Gaming Laptop",
            Price = 65000,
            Stock = 10
        };

        var productDto = new ProductDto
        {
            Id = 1,
            Name = "Laptop",
            Description = "Gaming Laptop",
            Price = 65000,
            Stock = 10
        };

        var productRepositoryMock = new Mock<IProductRepository>();
        var auditRepositoryMock = new Mock<IAuditRepository>();

        productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ProductService.Models.Product>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        _unitOfWorkMock
    .Setup(x => x.AuditLogs)
    .Returns(auditRepositoryMock.Object);

        _mapperMock
            .Setup(x => x.Map<ProductService.Models.Product>(createDto))
            .Returns(product);

        _mapperMock
            .Setup(x => x.Map<ProductDto>(product))
            .Returns(productDto);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act

        var result = await _service.AddProductAsync(createDto);

        // Assert

        result.Should().NotBeNull();

        result.Name.Should().Be("Laptop");

        result.Price.Should().Be(65000);

        _mapperMock.Verify(
            x => x.Map<ProductService.Models.Product>(createDto),
            Times.Once);

        productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ProductService.Models.Product>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _mapperMock.Verify(
            x => x.Map<ProductDto>(product),
            Times.Once);
        auditRepositoryMock.Verify(
    x => x.AddAsync(It.IsAny<AuditLog>()),
    Times.Once);
    }
    [Fact]
    public async Task UpdateProductAsync_WhenProductExists_ShouldUpdateAndReturnDto()
    {
        // Arrange

        var existingProduct = new ProductService.Models.Product
        {
            Id = 1,
            Name = "Old Product",
            Description = "Old Description",
            Price = 100,
            Stock = 10,
            ImageUrl = "old.jpg",
            CategoryId = 1
        };

        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 200,
            Stock = 20,
            ImageUrl = "new.jpg",
            CategoryId = 2
        };


        var updatedProductDto = new ProductDto
        {
            Id = 1,
            Name = "Updated Product",
            Price = 200,
            Stock = 20
        };


        var productRepositoryMock = new Mock<IProductRepository>();

        var auditRepositoryMock = new Mock<IAuditRepository>();


        productRepositoryMock
    .Setup(x => x.GetByIdAsync(1))
    .ReturnsAsync(existingProduct);


        productRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductService.Models.Product>()))
            .ReturnsAsync(existingProduct);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _unitOfWorkMock
            .Setup(x => x.AuditLogs)
            .Returns(auditRepositoryMock.Object);


        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        _mapperMock
            .Setup(x => x.Map<ProductDto>(It.IsAny<ProductService.Models.Product>()))
            .Returns(updatedProductDto);


        auditRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);


        // Act

        var result = await _service.UpdateProductAsync(1, updateDto);


        // Assert

        result.Should().NotBeNull();

        result!.Name.Should().Be("Updated Product");

        result.Price.Should().Be(200);


        productRepositoryMock.Verify(
            x => x.GetByIdAsync(1),
            Times.Once);


        productRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<ProductService.Models.Product>(p =>
                p.Name == "Updated Product" &&
                p.Price == 200 &&
                p.Stock == 20)),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);


        auditRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuditLog>()),
            Times.Once);


        _mapperMock.Verify(
            x => x.Map<ProductDto>(existingProduct),
            Times.Once);
    }
    [Fact]
    public async Task UpdateProductAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange

        int productId = 100;

        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 200,
            Stock = 20,
            ImageUrl = "new.jpg",
            CategoryId = 2
        };


        var productRepositoryMock = new Mock<IProductRepository>();


        productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((ProductService.Models.Product?)null);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        // Act

        var result = await _service.UpdateProductAsync(productId, updateDto);


        // Assert

        result.Should().BeNull();


        productRepositoryMock.Verify(
            x => x.GetByIdAsync(productId),
            Times.Once);


        productRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<ProductService.Models.Product>()),
            Times.Never);


        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);


        _mapperMock.Verify(
            x => x.Map<ProductDto>(It.IsAny<ProductService.Models.Product>()),
            Times.Never);
    }
    [Fact]
    public async Task DeleteProductAsync_Should_Delete_Product_And_Commit_Transaction()
    {
        // Arrange

        int productId = 1;

        var productRepositoryMock = new Mock<IProductRepository>();

        var auditRepositoryMock = new Mock<IAuditRepository>();


        productRepositoryMock
            .Setup(x => x.DeleteAsync(productId))
            .Returns(Task.CompletedTask);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _unitOfWorkMock
            .Setup(x => x.AuditLogs)
            .Returns(auditRepositoryMock.Object);


        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);


        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync())
            .Returns(Task.CompletedTask);


        auditRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);


        // Act

        await _service.DeleteProductAsync(productId);


        // Assert

        productRepositoryMock.Verify(
            x => x.DeleteAsync(productId),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(),
            Times.Once);


        auditRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuditLog>()),
            Times.Once);
    }
    [Fact]
    public async Task DeleteProductAsync_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange

        int productId = 1;

        var productRepositoryMock = new Mock<IProductRepository>();


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);


        productRepositoryMock
            .Setup(x => x.DeleteAsync(productId))
            .ThrowsAsync(new Exception("Delete failed"));


        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);


        // Act

        Func<Task> act = async () =>
        {
            await _service.DeleteProductAsync(productId);
        };


        // Assert

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Delete failed");


        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(),
            Times.Once);


        productRepositoryMock.Verify(
            x => x.DeleteAsync(productId),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(),
            Times.Never);


        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
    [Fact]
    public async Task SearchProductsAsync_WhenProductsMatch_ShouldReturnProducts()
    {
        // Arrange

        string keyword = "Laptop";


        var products = new List<ProductService.Models.Product>
    {
        new ProductService.Models.Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 50000
        },
        new ProductService.Models.Product
        {
            Id = 2,
            Name = "Laptop Bag",
            Price = 2000
        }
    };


        var productDtos = new List<ProductDto>
    {
        new ProductDto
        {
            Id = 1,
            Name = "Laptop",
            Price = 50000
        },
        new ProductDto
        {
            Id = 2,
            Name = "Laptop Bag",
            Price = 2000
        }
    };


        var productRepositoryMock = new Mock<IProductRepository>();


        productRepositoryMock
            .Setup(x => x.SearchProductsAsync(keyword))
            .ReturnsAsync(products);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);


        // Act

        var result = await _service.SearchProductsAsync(keyword);


        // Assert

        result.Should().NotBeNull();

        result.Count().Should().Be(2);

        result.First().Name.Should().Be("Laptop");


        productRepositoryMock.Verify(
            x => x.SearchProductsAsync(keyword),
            Times.Once);


        _mapperMock.Verify(
            x => x.Map<IEnumerable<ProductDto>>(products),
            Times.Once);
    }
    [Fact]
    public async Task SearchProductsAsync_WhenNoProductsFound_ShouldReturnEmptyList()
    {
        // Arrange

        string keyword = "Mobile";


        var products = new List<ProductService.Models.Product>();


        var productDtos = new List<ProductDto>();


        var productRepositoryMock = new Mock<IProductRepository>();


        productRepositoryMock
            .Setup(x => x.SearchProductsAsync(keyword))
            .ReturnsAsync(products);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);


        // Act

        var result = await _service.SearchProductsAsync(keyword);


        // Assert

        result.Should().NotBeNull();

        result.Should().BeEmpty();


        productRepositoryMock.Verify(
            x => x.SearchProductsAsync(keyword),
            Times.Once);


        _mapperMock.Verify(
            x => x.Map<IEnumerable<ProductDto>>(products),
            Times.Once);
    }
    [Fact]
    public async Task GetPagedProductsAsync_Should_Return_PagedProducts()
    {
        // Arrange

        int pageNumber = 1;
        int pageSize = 2;


        var products = new List<ProductService.Models.Product>
    {
        new ProductService.Models.Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 50000
        },
        new ProductService.Models.Product
        {
            Id = 2,
            Name = "Mouse",
            Price = 1000
        }
    };


        var productDtos = new List<ProductDto>
    {
        new ProductDto
        {
            Id = 1,
            Name = "Laptop",
            Price = 50000
        },
        new ProductDto
        {
            Id = 2,
            Name = "Mouse",
            Price = 1000
        }
    };


        var productRepositoryMock = new Mock<IProductRepository>();


        productRepositoryMock
            .Setup(x => x.GetPagedProductsAsync(pageNumber, pageSize))
            .ReturnsAsync(products);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);


        // Act

        var result = await _service.GetPagedProductsAsync(pageNumber, pageSize);


        // Assert

        result.Should().NotBeNull();

        result.Count().Should().Be(2);

        result.First().Name.Should().Be("Laptop");


        productRepositoryMock.Verify(
            x => x.GetPagedProductsAsync(pageNumber, pageSize),
            Times.Once);


        _mapperMock.Verify(
            x => x.Map<IEnumerable<ProductDto>>(products),
            Times.Once);
    }
    [Fact]
    public async Task GetPagedProductsAsync_WhenNoProducts_Should_ReturnEmptyList()
    {
        // Arrange

        int pageNumber = 1;
        int pageSize = 10;


        var products = new List<ProductService.Models.Product>();


        var productDtos = new List<ProductDto>();


        var productRepositoryMock = new Mock<IProductRepository>();


        productRepositoryMock
            .Setup(x => x.GetPagedProductsAsync(pageNumber, pageSize))
            .ReturnsAsync(products);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);


        // Act

        var result = await _service.GetPagedProductsAsync(pageNumber, pageSize);


        // Assert

        result.Should().NotBeNull();

        result.Should().BeEmpty();


        productRepositoryMock.Verify(
            x => x.GetPagedProductsAsync(pageNumber, pageSize),
            Times.Once);


        _mapperMock.Verify(
            x => x.Map<IEnumerable<ProductDto>>(products),
            Times.Once);
    }
    [Fact]
    public async Task GetProductsByCategoryAsync_WhenProductsExist_ShouldReturnProducts()
    {
        // Arrange

        int categoryId = 1;


        var products = new List<ProductService.Models.Product>
    {
        new ProductService.Models.Product
        {
            Id = 1,
            Name = "Laptop",
            CategoryId = categoryId,
            Price = 50000
        },
        new ProductService.Models.Product
        {
            Id = 2,
            Name = "Keyboard",
            CategoryId = categoryId,
            Price = 2000
        }
    };


        var productDtos = new List<ProductDto>
    {
        new ProductDto
        {
            Id = 1,
            Name = "Laptop",
            Price = 50000
        },
        new ProductDto
        {
            Id = 2,
            Name = "Keyboard",
            Price = 2000
        }
    };


        var productRepositoryMock = new Mock<IProductRepository>();


        productRepositoryMock
            .Setup(x => x.GetProductsByCategoryAsync(categoryId))
            .ReturnsAsync(products);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);


        // Act

        var result = await _service.GetProductsByCategoryAsync(categoryId);


        // Assert

        result.Should().NotBeNull();

        result.Count().Should().Be(2);

        result.First().Name.Should().Be("Laptop");


        productRepositoryMock.Verify(
            x => x.GetProductsByCategoryAsync(categoryId),
            Times.Once);


        _mapperMock.Verify(
            x => x.Map<IEnumerable<ProductDto>>(products),
            Times.Once);
    }
    [Fact]
    public async Task GetProductsByCategoryAsync_WhenNoProducts_ShouldReturnEmptyList()
    {
        // Arrange

        int categoryId = 999;


        var products = new List<ProductService.Models.Product>();

        var productDtos = new List<ProductDto>();


        var productRepositoryMock = new Mock<IProductRepository>();


        productRepositoryMock
            .Setup(x => x.GetProductsByCategoryAsync(categoryId))
            .ReturnsAsync(products);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _mapperMock
            .Setup(x => x.Map<IEnumerable<ProductDto>>(products))
            .Returns(productDtos);


        // Act

        var result = await _service.GetProductsByCategoryAsync(categoryId);


        // Assert

        result.Should().NotBeNull();

        result.Should().BeEmpty();


        productRepositoryMock.Verify(
            x => x.GetProductsByCategoryAsync(categoryId),
            Times.Once);


        _mapperMock.Verify(
            x => x.Map<IEnumerable<ProductDto>>(products),
            Times.Once);
    }
    [Fact]
    public async Task UploadImageAsync_WhenValidFile_ShouldReturnImageUrl()
    {
        // Arrange

        var fileMock = new Mock<IFormFile>();

        var imageUrl = "images/product1.jpg";


        var productRepositoryMock = new Mock<IProductRepository>();


        productRepositoryMock
            .Setup(x => x.UploadImageAsync(fileMock.Object))
            .ReturnsAsync(imageUrl);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        // Act

        var result = await _service.UploadImageAsync(fileMock.Object);


        // Assert

        result.Should().NotBeNull();

        result.Should().Be(imageUrl);


        productRepositoryMock.Verify(
            x => x.UploadImageAsync(fileMock.Object),
            Times.Once);
    }
    [Fact]
    public async Task BulkInsertAsync_WhenProductsProvided_ShouldInsertProducts()
    {
        // Arrange

        var dto = new BulkProductDto
        {
            Products = new List<CreateProductDto>
        {
            new CreateProductDto
            {
                Name = "Laptop",
                Description = "Gaming Laptop",
                Price = 60000,
                Stock = 10,
                ImageUrl = "laptop.jpg",
                CategoryId = 1
            },
            new CreateProductDto
            {
                Name = "Mouse",
                Description = "Wireless Mouse",
                Price = 1000,
                Stock = 50,
                ImageUrl = "mouse.jpg",
                CategoryId = 2
            }
        }
        };


        var productRepositoryMock = new Mock<IProductRepository>();

        var auditRepositoryMock = new Mock<IAuditRepository>();


        productRepositoryMock
            .Setup(x => x.BulkInsertAsync(It.IsAny<List<ProductService.Models.Product>>()))
            .Returns(Task.CompletedTask);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _unitOfWorkMock
            .Setup(x => x.AuditLogs)
            .Returns(auditRepositoryMock.Object);


        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);


        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync())
            .Returns(Task.CompletedTask);


        auditRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);


        // Act

        await _service.BulkInsertAsync(dto);


        // Assert

        productRepositoryMock.Verify(
            x => x.BulkInsertAsync(It.Is<List<ProductService.Models.Product>>(p =>
                p.Count == 2 &&
                p[0].Name == "Laptop" &&
                p[1].Name == "Mouse")),
            Times.Once);


        auditRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuditLog>()),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(),
            Times.Once);
    }
    [Fact]
    public async Task BulkInsertAsync_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange

        var dto = new BulkProductDto
        {
            Products = new List<CreateProductDto>
        {
            new CreateProductDto
            {
                Name = "Laptop",
                Price = 60000,
                Stock = 10
            }
        }
        };


        var productRepositoryMock = new Mock<IProductRepository>();


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);


        productRepositoryMock
            .Setup(x => x.BulkInsertAsync(It.IsAny<List<ProductService.Models.Product>>()))
            .ThrowsAsync(new Exception("Bulk insert failed"));


        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);


        // Act

        Func<Task> act = async () =>
        {
            await _service.BulkInsertAsync(dto);
        };


        // Assert

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Bulk insert failed");


        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(),
            Times.Once);


        productRepositoryMock.Verify(
            x => x.BulkInsertAsync(It.IsAny<List<ProductService.Models.Product>>()),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(),
            Times.Never);


        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
    [Fact]
    public async Task BulkUpdateAsync_WhenProductsProvided_ShouldUpdateProducts()
    {
        // Arrange

        var dto = new UpdateBulkProductDto
        {
            Products = new List<UpdateProductDtoWithId>
{
    new UpdateProductDtoWithId
    {
        Id = 1,
        Name = "Updated Laptop",
        Description = "New Description",
        Price = 70000,
        Stock = 20,
        ImageUrl = "new.jpg",
        CategoryId = 2
    },
    new UpdateProductDtoWithId
    {
        Id = 2,
        Name = "Updated Mouse",
        Description = "Wireless",
        Price = 2000,
        Stock = 50,
        ImageUrl = "mouse.jpg",
        CategoryId = 3
    }
}
        };


        var product1 = new ProductService.Models.Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 50000,
            Stock = 10
        };


        var product2 = new ProductService.Models.Product
        {
            Id = 2,
            Name = "Mouse",
            Price = 1000,
            Stock = 20
        };


        var productRepositoryMock = new Mock<IProductRepository>();

        var auditRepositoryMock = new Mock<IAuditRepository>();


        productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product1);


        productRepositoryMock
            .Setup(x => x.GetByIdAsync(2))
            .ReturnsAsync(product2);


        productRepositoryMock
            .Setup(x => x.BulkUpdateAsync(It.IsAny<List<ProductService.Models.Product>>()))
            .Returns(Task.CompletedTask);


        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);


        _unitOfWorkMock
            .Setup(x => x.AuditLogs)
            .Returns(auditRepositoryMock.Object);


        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);


        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync())
            .Returns(Task.CompletedTask);


        auditRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);


        // Act

        await _service.BulkUpdateAsync(dto);


        // Assert

        productRepositoryMock.Verify(
            x => x.GetByIdAsync(1),
            Times.Once);


        productRepositoryMock.Verify(
            x => x.GetByIdAsync(2),
            Times.Once);


        productRepositoryMock.Verify(
            x => x.BulkUpdateAsync(It.Is<List<ProductService.Models.Product>>(p =>
                p.Count == 2 &&
                p[0].Name == "Updated Laptop" &&
                p[1].Name == "Updated Mouse")),
            Times.Once);


        auditRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuditLog>()),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);


        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(),
            Times.Once);
    }
    [Fact]
    public async Task BulkUpdateAsync_WhenProductDoesNotExist_ShouldSkipProduct()
    {
        // Arrange

        var dto = new UpdateBulkProductDto
        {
            Products = new List<UpdateProductDtoWithId>
        {
            new UpdateProductDtoWithId
            {
                Id = 1,
                Name = "Laptop Updated",
                Description = "Gaming",
                Price = 70000,
                Stock = 20,
                CategoryId = 1
            },
            new UpdateProductDtoWithId
            {
                Id = 2,
                Name = "Mouse Updated",
                Description = "Wireless",
                Price = 1500,
                Stock = 30,
                CategoryId = 2
            }
        }
        };

        var existingProduct = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 50000
        };

        var productRepositoryMock = new Mock<IProductRepository>();
        var auditRepositoryMock = new Mock<IAuditRepository>();

        productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingProduct);

        productRepositoryMock
            .Setup(x => x.GetByIdAsync(2))
            .ReturnsAsync((Product?)null);

        productRepositoryMock
            .Setup(x => x.BulkUpdateAsync(It.IsAny<List<Product>>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.AuditLogs)
            .Returns(auditRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync())
            .Returns(Task.CompletedTask);

        auditRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act

        await _service.BulkUpdateAsync(dto);

        // Assert

        productRepositoryMock.Verify(
            x => x.BulkUpdateAsync(It.Is<List<Product>>(p =>
                p.Count == 1 &&
                p[0].Id == 1)),
            Times.Once);
    }
    [Fact]
    public async Task BulkUpdateAsync_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange

        var dto = new UpdateBulkProductDto
        {
            Products = new List<UpdateProductDtoWithId>
        {
            new UpdateProductDtoWithId
            {
                Id = 1,
                Name = "Laptop",
                Description = "Gaming",
                Price = 60000,
                Stock = 10,
                CategoryId = 1
            }
        }
        };

        var existingProduct = new Product
        {
            Id = 1,
            Name = "Old Laptop",
            Price = 50000
        };

        var productRepositoryMock = new Mock<IProductRepository>();

        productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingProduct);

        productRepositoryMock
            .Setup(x => x.BulkUpdateAsync(It.IsAny<List<Product>>()))
            .ThrowsAsync(new Exception("Bulk update failed"));

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);

        // Act

        Func<Task> act = async () =>
        {
            await _service.BulkUpdateAsync(dto);
        };

        // Assert

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Bulk update failed");

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
    [Fact]
    public async Task BulkDeleteAsync_WhenProductIdsProvided_ShouldDeleteProducts()
    {
        // Arrange

        var dto = new BulkDeleteDto
        {
            ProductIds = new List<int> { 1, 2, 3 }
        };

        var productRepositoryMock = new Mock<IProductRepository>();
        var auditRepositoryMock = new Mock<IAuditRepository>();

        productRepositoryMock
            .Setup(x => x.BulkDeleteAsync(dto.ProductIds))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.AuditLogs)
            .Returns(auditRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync())
            .Returns(Task.CompletedTask);

        auditRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act

        await _service.BulkDeleteAsync(dto);

        // Assert

        productRepositoryMock.Verify(
            x => x.BulkDeleteAsync(dto.ProductIds),
            Times.Once);

        auditRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuditLog>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(),
            Times.Once);
    }
    [Fact]
    public async Task BulkDeleteAsync_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        // Arrange

        var dto = new BulkDeleteDto
        {
            ProductIds = new List<int> { 1, 2, 3 }
        };

        var productRepositoryMock = new Mock<IProductRepository>();

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        productRepositoryMock
            .Setup(x => x.BulkDeleteAsync(It.IsAny<List<int>>()))
            .ThrowsAsync(new Exception("Bulk delete failed"));

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);

        // Act

        Func<Task> act = async () =>
        {
            await _service.BulkDeleteAsync(dto);
        };

        // Assert

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Bulk delete failed");

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(),
            Times.Once);

        productRepositoryMock.Verify(
            x => x.BulkDeleteAsync(It.IsAny<List<int>>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
    [Fact]
    public async Task ImportProductsAsync_WhenFileIsNull_ShouldThrowException()
    {
        // Act

        Func<Task> act = async () =>
        {
            await _service.ImportProductsAsync(null!);
        };

        // Assert

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Please upload an Excel file.");

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
    [Fact]
    public async Task ImportProductsAsync_WhenValidExcel_ShouldImportProducts()
    {
        // Arrange

        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Products");

        // Header row
        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Description";
        worksheet.Cell(1, 4).Value = "Price";
        worksheet.Cell(1, 5).Value = "Stock";
        worksheet.Cell(1, 6).Value = "CategoryId";
        worksheet.Cell(1, 7).Value = "ImageUrl";

        // Product row
        worksheet.Cell(2, 2).Value = "Laptop";
        worksheet.Cell(2, 3).Value = "Gaming Laptop";
        worksheet.Cell(2, 4).Value = 65000;
        worksheet.Cell(2, 5).Value = 10;
        worksheet.Cell(2, 6).Value = 1;
        worksheet.Cell(2, 7).Value = "laptop.jpg";

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        stream.Position = 0;

        var fileMock = new Mock<IFormFile>();

        fileMock.Setup(f => f.Length)
                .Returns(stream.Length);

        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns<Stream, CancellationToken>(async (target, token) =>
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(target, token);
                });

        var productRepositoryMock = new Mock<IProductRepository>();

        var auditRepositoryMock = new Mock<IAuditRepository>();

        productRepositoryMock
            .Setup(x => x.ImportProductsAsync(It.IsAny<List<Product>>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.AuditLogs)
            .Returns(auditRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync())
            .Returns(Task.CompletedTask);

        auditRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        // Act

        await _service.ImportProductsAsync(fileMock.Object);

        // Assert

        productRepositoryMock.Verify(
            x => x.ImportProductsAsync(It.Is<List<Product>>(p =>
                p.Count == 1 &&
                p[0].Name == "Laptop" &&
                p[0].Description == "Gaming Laptop" &&
                p[0].Price == 65000 &&
                p[0].Stock == 10 &&
                p[0].CategoryId == 1 &&
                p[0].ImageUrl == "laptop.jpg")),
            Times.Once);

        auditRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuditLog>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(),
            Times.Once);
    }
    [Fact]
    public async Task ImportProductsAsync_WhenRepositoryThrows_ShouldRollbackTransaction()
    {
        // Arrange

        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Products");

        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Description";
        worksheet.Cell(1, 4).Value = "Price";
        worksheet.Cell(1, 5).Value = "Stock";
        worksheet.Cell(1, 6).Value = "CategoryId";
        worksheet.Cell(1, 7).Value = "ImageUrl";

        worksheet.Cell(2, 2).Value = "Laptop";
        worksheet.Cell(2, 3).Value = "Gaming Laptop";
        worksheet.Cell(2, 4).Value = 50000;
        worksheet.Cell(2, 5).Value = 10;
        worksheet.Cell(2, 6).Value = 1;
        worksheet.Cell(2, 7).Value = "laptop.jpg";

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        stream.Position = 0;

        var fileMock = new Mock<IFormFile>();

        fileMock.Setup(f => f.Length)
                .Returns(stream.Length);

        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns<Stream, CancellationToken>(async (target, token) =>
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(target, token);
                });

        var productRepositoryMock = new Mock<IProductRepository>();

        productRepositoryMock
            .Setup(x => x.ImportProductsAsync(It.IsAny<List<Product>>()))
            .ThrowsAsync(new Exception("Import failed"));

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);

        // Act

        Func<Task> act = async () =>
        {
            await _service.ImportProductsAsync(fileMock.Object);
        };

        // Assert

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Import failed");

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
    [Fact]
    public async Task GetDashboardAsync_ShouldReturnDashboard()
    {
        // Arrange

        var dashboard = new DashboardDto
        {
            TotalProducts = 100,
            TotalCategories = 5,
            TotalUsers = 25,
            TotalStock = 2500,
            TotalReviews = 150,
            TotalWishlists = 75,
            OutOfStockProducts = 3,
            LowStockProducts = 10,
            AveragePrice = 2500,
            HighestPrice = 75000,
            LowestPrice = 500
        };

        var productRepositoryMock = new Mock<IProductRepository>();

        productRepositoryMock
            .Setup(x => x.GetDashboardAsync())
            .ReturnsAsync(dashboard);

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        // Act

        var result = await _service.GetDashboardAsync();

        // Assert

        result.Should().NotBeNull();

        result.TotalProducts.Should().Be(100);
        result.TotalCategories.Should().Be(5);
        result.TotalUsers.Should().Be(25);
        result.TotalStock.Should().Be(2500);
        result.TotalReviews.Should().Be(150);
        result.TotalWishlists.Should().Be(75);
        result.OutOfStockProducts.Should().Be(3);
        result.LowStockProducts.Should().Be(10);
        result.AveragePrice.Should().Be(2500);
        result.HighestPrice.Should().Be(75000);
        result.LowestPrice.Should().Be(500);

        productRepositoryMock.Verify(
            x => x.GetDashboardAsync(),
            Times.Once);
    }
}
