using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using System.Linq;
namespace ProductService.Repositories;
using ProductService.DTOs;

using ProductService.Repositories.Generic;

public class ProductRepository
    : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ProductDbContext context)
     : base(context)
    {
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new Exception("No file selected.");

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "images");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = Guid.NewGuid() +
                       Path.GetExtension(file.FileName);

        var filePath = Path.Combine(
            uploadsFolder,
            fileName);

        using var stream = new FileStream(
            filePath,
            FileMode.Create);

        await file.CopyToAsync(stream);

        return $"images/{fileName}";
    }

    public async Task<IEnumerable<Product>> FilterProductsAsync(decimal minPrice, decimal maxPrice)
    {
        return await _context.Products
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToListAsync();
    }
    public async Task<IEnumerable<Product>> SortProductsAsync(string sortBy)
    {
        IQueryable<Product> query = _context.Products;

        query = sortBy.ToLower() switch
        {
            "price" => query.OrderBy(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            "stock" => query.OrderBy(p => p.Stock),
            _ => query.OrderBy(p => p.Id)
        };

        return await query.ToListAsync();
    }
    public async Task<IEnumerable<Product>> GetPagedProductsAsync(int pageNumber, int pageSize)
    {
        return await _context.Products
            .OrderBy(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    public async Task<IEnumerable<Product>> SearchProductsAsync(string keyword)
    {
        return await _context.Products
            .Where(p => p.Name.Contains(keyword))
            .ToListAsync();
    }

    

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }


    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .ToListAsync();
    }
    public override async Task<Product?> GetByIdAsync(int id)
    {
        Console.WriteLine($"Searching Product Id = {id}");

        var product = await _context.Products
            .IgnoreQueryFilters()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            Console.WriteLine("PRODUCT = NULL");
        }
        else
        {
            Console.WriteLine($"FOUND PRODUCT: {product.Id} - {product.Name}");
            Console.WriteLine($"Category = {product.Category?.Name}");
        }

        return product;
    }




    public async Task BulkInsertAsync(List<Product> products)
    {
        await _context.Products.AddRangeAsync(products);
    }

    public async Task BulkUpdateAsync(List<Product> products)
    {
        _context.Products.UpdateRange(products);
    }

    public async Task BulkDeleteAsync(List<int> productIds)
    {
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        _context.Products.RemoveRange(products);

       
    }

    public async Task ImportProductsAsync(List<Product> products)
    {
        await _context.Products.AddRangeAsync(products);
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        return new DashboardDto
        {
            TotalProducts = await _context.Products.CountAsync(),

            TotalCategories = await _context.Categories.CountAsync(),

            TotalUsers = await _context.Users.CountAsync(),

            TotalReviews = await _context.Reviews.CountAsync(),

            TotalWishlists = await _context.Wishlists.CountAsync(),

            LowStockProducts = await _context.Products
                                             .CountAsync(p => p.Stock < 10)
        };
    }
   
}