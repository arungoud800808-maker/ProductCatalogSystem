using ProductService.DTOs;

namespace ProductService.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    Task<string> UploadImageAsync(IFormFile file);
    Task<IEnumerable<ProductDto>> FilterProductsAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<ProductDto>> SortProductsAsync(string sortBy);
    Task<IEnumerable<ProductDto>> GetPagedProductsAsync(int pageNumber, int pageSize);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword);
    Task<IEnumerable<ProductDto>> GetProductsAsync();
    Task<IEnumerable<ProductV2Dto>> GetProductsV2Async();

    Task<ProductDto?> GetProductAsync(int id);

    Task<ProductDto> AddProductAsync(CreateProductDto dto);

    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);

    Task BulkInsertAsync(BulkProductDto dto);

    Task BulkUpdateAsync(UpdateBulkProductDto dto);
    Task BulkDeleteAsync(BulkDeleteDto dto);
    Task<byte[]> ExportProductsAsync();

    Task ImportProductsAsync(IFormFile file);
    Task<DashboardDto> GetDashboardAsync();
    Task DeleteProductAsync(int id);
   
}