using ProductService.Data;
using ProductService.Models;
using StackExchange.Redis;

namespace ProductService.Services
{
    public class AuditService : IAuditService
    {
        private readonly ProductDbContext _context;

        public AuditService(ProductDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
    string email,
    string role,
    string action,
    string entityName,
    string details,
    string? ipAddress)
        {
            var audit = new AuditLog
            {
                UserEmail = email,
                Role=role,
                Action = action,
                EntityName = entityName,
                Details = details,
                IPAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(audit);

            await _context.SaveChangesAsync();
        }
    }
}