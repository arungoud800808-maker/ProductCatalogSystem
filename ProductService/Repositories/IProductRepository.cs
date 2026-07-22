using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Repositories;

using ProductService.Repositories.Generic;

public interface IProductRepository : IGenericRepository<Product>

{
    Task<string> UploadImageAsync(IFormFile file);
    Task<IEnumerable<Product>> FilterProductsAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<Product>> SortProductsAsync(string sortBy);
    Task<IEnumerable<Product>> GetPagedProductsAsync(int pageNumber, int pageSize);

    Task<IEnumerable<Product>> SearchProductsAsync(string keyword);
   
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

    Task BulkInsertAsync(List<Product> products);
    Task BulkUpdateAsync(List<Product> products);
    Task BulkDeleteAsync(List<int> productIds);

    Task ImportProductsAsync(List<Product> products);
    Task<DashboardDto> GetDashboardAsync();
  
}