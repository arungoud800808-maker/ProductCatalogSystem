namespace ProductService.DTOs;

public class DashboardDto
{
    public int TotalProducts { get; set; }

    public int TotalCategories { get; set; }

    public int TotalUsers { get; set; }

    public int TotalStock { get; set; }

    public int TotalReviews { get; set; }

    public int TotalWishlists { get; set; }


    public int OutOfStockProducts { get; set; }

    public int LowStockProducts { get; set; }

    public decimal AveragePrice { get; set; }

    public decimal HighestPrice { get; set; }

    public decimal LowestPrice { get; set; }
}