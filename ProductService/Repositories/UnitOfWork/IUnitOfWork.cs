using ProductService.Models;
using ProductService.Repositories.Generic;
using ProductService.Repositories.UnitOfWork;
namespace ProductService.Repositories.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    // Repositories
    IProductRepository Products { get; }

    ICategoryRepository Categories { get; }

    IReviewRepository Reviews { get; }

    IWishlistRepository Wishlists { get; }

    IAuditRepository AuditLogs { get; }

    // Generic Repositories
    IGenericRepository<Product> ProductGeneric { get; }

    IGenericRepository<Category> CategoryGeneric { get; }

    IGenericRepository<User> UserGeneric { get; }

    // Database Operations
    Task<int> SaveChangesAsync();

    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();

}