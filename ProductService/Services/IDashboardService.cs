using ProductService.DTOs;

namespace ProductService.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}