namespace ProductService.Models;

public class AuditLog
{
    public int Id { get; set; }

    public string UserEmail { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? IPAddress { get; set; }

    public string Role { get; set; } = string.Empty;
}