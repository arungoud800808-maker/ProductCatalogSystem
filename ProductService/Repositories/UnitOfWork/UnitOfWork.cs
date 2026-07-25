using Microsoft.EntityFrameworkCore.Storage;
using ProductService.Data;
using ProductService.Models;
using ProductService.Repositories.Generic;

namespace ProductService.Repositories.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    private readonly ProductDbContext _context;

    public IProductRepository Products { get; }

    public ICategoryRepository Categories { get; }

    public IReviewRepository Reviews { get; }

    public IWishlistRepository Wishlists { get; }

    public IGenericRepository<Product> ProductGeneric { get; }

    public IGenericRepository<Category> CategoryGeneric { get; }

    public IGenericRepository<User> UserGeneric { get; }

    public IAuditRepository AuditLogs { get; }

    public UnitOfWork(
        ProductDbContext context,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IReviewRepository reviewRepository,
        IWishlistRepository wishlistRepository,
        IGenericRepository<Product> productGeneric,
        IGenericRepository<Category> categoryGeneric,
        IGenericRepository<User> userGeneric,
        IAuditRepository auditRepository)
    {
        _context = context;

        Products = productRepository;
        Categories = categoryRepository;
        Reviews = reviewRepository;
        Wishlists = wishlistRepository;

        ProductGeneric = productGeneric;
        CategoryGeneric = categoryGeneric;
        UserGeneric = userGeneric;

        AuditLogs = auditRepository;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        await BeginTransactionAsync();

        try
        {
            await operation();

            await CommitTransactionAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }
}