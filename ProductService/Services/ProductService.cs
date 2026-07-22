using AutoMapper;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using ProductService.Constants;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Repositories.UnitOfWork;
using System.IO;
using System.Text.Json;
namespace ProductService.Services;

public class ProductService : IProductService
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public ProductService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMemoryCache cache,
    ILogger<ProductService> logger,
    ProductDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
        _context = context;
    }

    // Upload Product Image
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        return await _unitOfWork.Products.UploadImageAsync(file);
    }

    // Filter Products
    public async Task<IEnumerable<ProductDto>> FilterProductsAsync(decimal minPrice, decimal maxPrice)
    {
        var products = await _unitOfWork.Products.FilterProductsAsync(minPrice, maxPrice);

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // Sort Products
    public async Task<IEnumerable<ProductDto>> SortProductsAsync(string sortBy)
    {
        var products = await _unitOfWork.Products.SortProductsAsync(sortBy);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // Pagination
    public async Task<IEnumerable<ProductDto>> GetPagedProductsAsync(int pageNumber, int pageSize)
    {
        var products = await _unitOfWork.Products.GetPagedProductsAsync(pageNumber, pageSize);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // Search Products
    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword)
    {
        var products = await _unitOfWork.Products.SearchProductsAsync(keyword);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // Get All Products
    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        const string cacheKey = "PRODUCT_LIST";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<ProductDto>? cachedProducts))
        {
            _logger.LogInformation("Returned from Memory Cache");
            return cachedProducts!;
        }

        var products = await _unitOfWork.Products.GetAllAsync();
        _logger.LogInformation("Returned from SQL Server");

        var result = _mapper.Map<IEnumerable<ProductDto>>(products);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        _cache.Set(cacheKey, result, cacheOptions);

        return result;
    }
    // Get All Products (Version 2)
    public async Task<IEnumerable<ProductV2Dto>> GetProductsV2Async()
    {
        var products = await _unitOfWork.Products.GetAllAsync();

        return products.Select(p => new ProductV2Dto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            CreatedDate = p.CreatedDate
        });
    }
    // Get Product By Id
    public async Task<ProductDto?> GetProductAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return null;

        return _mapper.Map<ProductDto>(product);
    }

    // Add Product
    public async Task<ProductDto> AddProductAsync(CreateProductDto dto)
    {
        var product = _mapper.Map<Product>(dto);

        await _unitOfWork.Products.AddAsync(product);

        await LogAuditAsync(
            "Create",
            $"Created Product : {product.Name}");

        await _unitOfWork.SaveChangesAsync();

        _cache.Remove(CacheKeys.ProductList);

        return _mapper.Map<ProductDto>(product);

     
    }

    // Update Product
    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return null;

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.ImageUrl = dto.ImageUrl;
        product.CategoryId = dto.CategoryId;

        await _unitOfWork.Products.UpdateAsync(product);

        await LogAuditAsync(
            "Update",
            $"Updated Product : {product.Name}");

        await _unitOfWork.SaveChangesAsync();

        _cache.Remove(CacheKeys.ProductList);

        return _mapper.Map<ProductDto>(product);
    }
    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task BulkInsertAsync(BulkProductDto dto)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var products = dto.Products.Select(x => new Product
                {
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Stock = x.Stock,
                    ImageUrl = x.ImageUrl,
                    CategoryId = x.CategoryId
                }).ToList();
                await _unitOfWork.Products.BulkInsertAsync(products);

                await LogAuditAsync(
                    "Bulk Insert",
                    $"{products.Count} products inserted successfully.");

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _cache.Remove(CacheKeys.ProductList);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        });
    }
    public async Task BulkUpdateAsync(UpdateBulkProductDto dto)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var products = new List<Product>();

                foreach (var item in dto.Products)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.Id);

                    if (product == null)
                        continue;

                    product.Name = item.Name;
                    product.Description = item.Description;
                    product.Price = item.Price;
                    product.Stock = item.Stock;
                    product.ImageUrl = item.ImageUrl;
                    product.CategoryId = item.CategoryId;

                    products.Add(product);
                }

                await _unitOfWork.Products.BulkUpdateAsync(products);

                await LogAuditAsync(
                    "Bulk Update",
                    $"{products.Count} products updated successfully.");

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _cache.Remove(CacheKeys.ProductList);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        });
    }
    public async Task BulkDeleteAsync(BulkDeleteDto dto)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.Products.BulkDeleteAsync(dto.ProductIds);

                await LogAuditAsync(
                    "Bulk Delete",
                    $"{dto.ProductIds.Count} products deleted successfully.");

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _cache.Remove(CacheKeys.ProductList);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        });
    }
    public async Task<byte[]> ExportProductsAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Products");

        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Description";
        worksheet.Cell(1, 4).Value = "Price";
        worksheet.Cell(1, 5).Value = "Stock";
        worksheet.Cell(1, 6).Value = "Category";
        worksheet.Cell(1, 7).Value = "Image";

        int row = 2;

        foreach (var product in products)
        {
            worksheet.Cell(row, 1).Value = product.Id;
            worksheet.Cell(row, 2).Value = product.Name;
            worksheet.Cell(row, 3).Value = product.Description;
            worksheet.Cell(row, 4).Value = product.Price;
            worksheet.Cell(row, 5).Value = product.Stock;
            worksheet.Cell(row, 6).Value = product.Category?.Name;
            worksheet.Cell(row, 7).Value = product.ImageUrl;

            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return stream.ToArray();
    }

    public async Task ImportProductsAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new Exception("Please upload an Excel file.");

        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var products = new List<Product>();

                using var stream = new MemoryStream();

                await file.CopyToAsync(stream);

                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);

                var worksheet = workbook.Worksheet(1);

                var rows = worksheet.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    products.Add(new Product
                    {
                        Name = row.Cell(2).GetString(),
                        Description = row.Cell(3).GetString(),
                        Price = row.Cell(4).GetValue<decimal>(),
                        Stock = row.Cell(5).GetValue<int>(),
                        CategoryId = row.Cell(6).GetValue<int>(),
                        ImageUrl = row.Cell(7).GetString()
                    });
                }

                await _unitOfWork.Products.ImportProductsAsync(products);

                await LogAuditAsync(
                    "Import Excel",
                    $"{products.Count} products imported from Excel.");

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _cache.Remove(CacheKeys.ProductList);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        });
    }
    public async Task<DashboardDto> GetDashboardAsync()
    {
        return await _unitOfWork.Products.GetDashboardAsync();
    }

    // Delete Product
    public async Task DeleteProductAsync(int id)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.Products.DeleteAsync(id);

                await LogAuditAsync(
                    "Delete",
                    $"Deleted Product Id : {id}");

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _cache.Remove(CacheKeys.ProductList);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        });
    }
    private async Task LogAuditAsync(
    string action,
    string details)
    {
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserEmail = "admin@gmail.com", // Replace later with logged-in user's email
            Action = action,
            EntityName = "Product",
            Details = details,
            CreatedAt = DateTime.UtcNow
        });
    }
}