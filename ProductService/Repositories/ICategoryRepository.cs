using ProductService.Models;

namespace ProductService.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task<Category> AddAsync(Category category);

    Task<Category?> UpdateAsync(Category category);

    Task DeleteAsync(Category category);
}