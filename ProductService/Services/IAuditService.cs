namespace ProductService.Services
{
    public interface IAuditService
    {
        Task LogAsync(
     string email,
     string role,
     string action,
     string entityName,
     string details,
     string? ipAddress);
    }
}