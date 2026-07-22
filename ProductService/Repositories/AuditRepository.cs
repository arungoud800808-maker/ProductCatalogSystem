using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

public class AuditRepository : IAuditRepository
{
    private readonly ProductDbContext _context;

    public AuditRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
    }

    public async Task<List<AuditLog>> GetAllAsync()
    {
        return await _context.AuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}