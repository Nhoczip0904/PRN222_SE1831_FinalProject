using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class RevenueDashboardDto
{
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }
    
    // Overall Statistics
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    
    // Category-wise Statistics
    public List<CategoryRevenueDto> CategoryRevenues { get; set; } = new();
    
    // Daily Statistics
    public List<DailyRevenueDto> DailyRevenues { get; set; } = new();
    
    // Top Products
    public List<TopProductDto> TopProducts { get; set; } = new();
    
    // Top Sellers
    public List<TopSellerDto> TopSellers { get; set; } = new();
}

public class CategoryRevenueDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Commission { get; set; }
    public int OrderCount { get; set; }
    public decimal CommissionRate { get; set; } // Usually 25%
}

public class DailyRevenueDto
{
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public decimal Commission { get; set; }
    public int OrderCount { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int QuantitySold { get; set; }
    public decimal Commission { get; set; }
}

public class TopSellerDto
{
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal Commission { get; set; }
}

public class RevenueSearchDto
{
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }
    public int? CategoryId { get; set; }
    public int? SellerId { get; set; }
    public string? GroupBy { get; set; } = "day"; // day, week, month
}
