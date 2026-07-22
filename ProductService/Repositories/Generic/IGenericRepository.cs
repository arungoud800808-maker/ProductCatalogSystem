using System.Linq.Expressions;

namespace ProductService.Repositories.Generic;
using ProductService.Specifications;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();

    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> ListAsync(ISpecification<T> specification);

    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    Task<T> AddAsync(T entity);

    Task<T?> UpdateAsync(T entity);

    Task DeleteAsync(int id);
}