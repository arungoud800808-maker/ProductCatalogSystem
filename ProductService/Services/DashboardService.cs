using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.DTOs;

namespace ProductService.Services;

public class DashboardService : IDashboardService
{
    private readonly ProductDbContext _context;

    public DashboardService(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var dashboard = new DashboardDto();

        dashboard.TotalProducts = await _context.Products.CountAsync();

        dashboard.TotalCategories = await _context.Categories.CountAsync();

        dashboard.TotalUsers = await _context.Users.CountAsync();

        dashboard.TotalStock = await _context.Products.SumAsync(p => p.Stock);

        dashboard.OutOfStockProducts =
            await _context.Products.CountAsync(p => p.Stock == 0);

        dashboard.LowStockProducts =
            await _context.Products.CountAsync(p => p.Stock < 10);

        if (await _context.Products.AnyAsync())
        {
            dashboard.AveragePrice =
                await _context.Products.AverageAsync(p => p.Price);

            dashboard.HighestPrice =
                await _context.Products.MaxAsync(p => p.Price);

            dashboard.LowestPrice =
                await _context.Products.MinAsync(p => p.Price);
        }

        return dashboard;
    }
}