using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using System.Linq.Expressions;

namespace ProductService.Repositories.Generic;
using ProductService.Specifications;
using ProductService.Interfaces;
public class GenericRepository<T> : IGenericRepository<T>
    where T : class
{
    protected readonly ProductDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ProductDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);

        return entity;
    }
    public virtual async Task<T?> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);

        return entity;
    }
    public virtual async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);

        if (entity == null)
            return;

        // Soft Delete
        if (entity is ISoftDelete softDeleteEntity)
        {
            softDeleteEntity.IsDeleted = true;
            softDeleteEntity.DeletedAt = DateTime.UtcNow;

            _dbSet.Update(entity);
        }
        else
        {
            // Fallback for entities that don't support soft delete
            _dbSet.Remove(entity);
        }

        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<T>> ListAsync(
     ISpecification<T> specification)
    {
        var query = SpecificationEvaluator<T>.GetQuery(
            _dbSet.AsQueryable(),
            specification);

        return await query.ToListAsync();
    }
}