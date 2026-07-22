using ProductService.Models;

public interface IAuditRepository
{
    Task AddAsync(AuditLog log);

    Task<List<AuditLog>> GetAllAsync();
}