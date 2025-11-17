using BLL.DTOs;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class RevenueService : IRevenueService
{
    private readonly EvBatteryTrading2Context _context;
    private const decimal COMMISSION_RATE = 0.25m; // 25% commission

    public RevenueService(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    private static DateTime GetFirstDayOfWeek(int year, int week)
    {
        // ISO week starts on Monday
        var jan1 = new DateTime(year, 1, 1);
        var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;
        var firstThursday = jan1.AddDays(daysOffset);
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        var firstWeek = cal.GetWeekOfYear(firstThursday, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        var weekNum = week;
        if (weekNum == 1 && firstWeek > 1)
        {
            weekNum = 53;
        }
        var result = firstThursday.AddDays(weekNum * 7);
        return result.AddDays(-3); // Back to Monday
    }

    public async Task<RevenueDashboardDto> GetRevenueDashboardAsync(RevenueSearchDto searchDto)
    {
        var startDate = searchDto.StartDate ?? DateTime.Today.AddDays(-30);
        var endDate = searchDto.EndDate ?? DateTime.Today;

        var dashboard = new RevenueDashboardDto
        {
            StartDate = startDate,
            EndDate = endDate
        };

        // Get overall statistics
        var overallStats = await GetOverallStatsAsync(startDate, endDate);
        dashboard.TotalRevenue = overallStats.Revenue;
        dashboard.TotalCommission = overallStats.Commission;
        dashboard.TotalOrders = overallStats.OrderCount;
        dashboard.AverageOrderValue = overallStats.OrderCount > 0 ? overallStats.Revenue / overallStats.OrderCount : 0;

        // Get category revenues
        dashboard.CategoryRevenues = await GetCategoryRevenuesAsync(startDate, endDate);

        // Get daily revenues
        dashboard.DailyRevenues = await GetDailyRevenuesAsync(startDate, endDate, searchDto.GroupBy ?? "day");

        // Get top products
        dashboard.TopProducts = await GetTopProductsAsync(startDate, endDate);

        // Get top sellers
        dashboard.TopSellers = await GetTopSellersAsync(startDate, endDate);

        return dashboard;
    }

    public async Task<List<CategoryRevenueDto>> GetCategoryRevenuesAsync(DateTime startDate, DateTime endDate)
    {
        // Ensure end date includes the entire day
        var adjustedEndDate = endDate.Date.AddDays(1).AddTicks(-1);
        
        var categoryRevenues = await (from order in _context.Orders
                                    join orderItem in _context.OrderItems on order.Id equals orderItem.OrderId
                                    join product in _context.Products on orderItem.ProductId equals product.Id
                                    join category in _context.Categories on product.CategoryId equals category.Id
                                    where order.CreatedAt >= startDate && order.CreatedAt <= adjustedEndDate
                                    && order.Status == "delivered"  // Only include delivered orders
                                    group new { order, orderItem, category } by new { category.Id, category.Name } into g
                                    select new CategoryRevenueDto
                                    {
                                        CategoryId = g.Key.Id,
                                        CategoryName = g.Key.Name,
                                        Revenue = g.Sum(x => x.orderItem.Quantity * x.orderItem.UnitPrice),
                                        OrderCount = g.Select(x => x.order.Id).Distinct().Count(),
                                        Commission = g.Sum(x => x.orderItem.Quantity * x.orderItem.UnitPrice) * COMMISSION_RATE
                                    })
                                    .ToListAsync();

        return categoryRevenues;
    }

    public async Task<List<DailyRevenueDto>> GetDailyRevenuesAsync(DateTime startDate, DateTime endDate, string groupBy = "day")
    {
        // Ensure end date includes the entire day
        var adjustedEndDate = endDate.Date.AddDays(1).AddTicks(-1);
        
        // Fetch data first to avoid EF Core translation issues
        // Only include delivered orders with valid CreatedAt
        var orders = await _context.Orders
            .Where(o => o.CreatedAt.HasValue 
                       && o.CreatedAt.Value >= startDate 
                       && o.CreatedAt.Value <= adjustedEndDate
                       && o.Status == "delivered")  // Only include delivered orders
            .ToListAsync();
        
        // Debug: Check if we have any orders
        Console.WriteLine($"[GetDailyRevenuesAsync] Date range: {startDate:dd/MM/yyyy HH:mm:ss} to {adjustedEndDate:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"[GetDailyRevenuesAsync] Total orders found: {orders.Count}");
        
        // Debug: Check order dates and show which ones are in range
        Console.WriteLine($"[GetDailyRevenuesAsync] Orders in date range:");
        foreach (var order in orders)
        {
            Console.WriteLine($"[GetDailyRevenuesAsync] Order {order.Id}: CreatedAt={order.CreatedAt:dd/MM/yyyy HH:mm:ss}, Status={order.Status}, Amount={order.TotalAmount}");
        }
        
        // Debug: Show orders outside range for comparison
        var allOrders = await _context.Orders.ToListAsync();
        var ordersOutsideRange = allOrders.Where(o => o.CreatedAt < startDate || o.CreatedAt > adjustedEndDate).Take(3);
        Console.WriteLine($"[GetDailyRevenuesAsync] Orders outside date range (sample):");
        foreach (var order in ordersOutsideRange)
        {
            Console.WriteLine($"[GetDailyRevenuesAsync] Order {order.Id}: CreatedAt={order.CreatedAt:dd/MM/yyyy HH:mm:ss}, Status={order.Status}, Amount={order.TotalAmount}");
        }

        IEnumerable<DailyRevenueDto> result;

        if (groupBy == "month")
        {
            result = orders
                .GroupBy(o => new { Year = o.CreatedAt.Value.Year, Month = o.CreatedAt.Value.Month })
                .Select(g => new DailyRevenueDto
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    Commission = g.Sum(o => o.TotalAmount) * COMMISSION_RATE
                });
        }
        else if (groupBy == "week")
        {
            result = orders
                .GroupBy(o => new { Year = o.CreatedAt.Value.Year, Week = System.Globalization.ISOWeek.GetWeekOfYear(o.CreatedAt.Value) })
                .Select(g => new DailyRevenueDto
                {
                    Date = GetFirstDayOfWeek(g.Key.Year, g.Key.Week),
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    Commission = g.Sum(o => o.TotalAmount) * COMMISSION_RATE
                });
        }
        else // day
        {
            result = orders
                .GroupBy(o => new DateTime(o.CreatedAt.Value.Year, o.CreatedAt.Value.Month, o.CreatedAt.Value.Day))
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    Commission = g.Sum(o => o.TotalAmount) * COMMISSION_RATE
                });
        }

        var finalResult = result.OrderBy(d => d.Date).ToList();
        Console.WriteLine($"[GetDailyRevenuesAsync] Final result count: {finalResult.Count}");
        
        return finalResult;
    }

    public async Task<List<TopProductDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int top = 10)
    {
        // Ensure end date includes the entire day
        var adjustedEndDate = endDate.Date.AddDays(1).AddTicks(-1);
        
        var topProducts = await (from order in _context.Orders
                                join orderItem in _context.OrderItems on order.Id equals orderItem.OrderId
                                join product in _context.Products on orderItem.ProductId equals product.Id
                                where order.CreatedAt >= startDate && order.CreatedAt <= adjustedEndDate
                                      && order.Status == "delivered"  // Only include delivered orders
                                group new { order, orderItem, product } by new { product.Id, product.Name } into g
                                select new TopProductDto
                                {
                                    ProductId = g.Key.Id,
                                    ProductName = g.Key.Name,
                                    Revenue = g.Sum(x => x.orderItem.Quantity * x.orderItem.UnitPrice),
                                    QuantitySold = g.Sum(x => x.orderItem.Quantity),
                                    Commission = g.Sum(x => x.orderItem.Quantity * x.orderItem.UnitPrice) * COMMISSION_RATE
                                })
                                .OrderByDescending(p => p.Revenue)
                                .Take(top)
                                .ToListAsync();

        return topProducts;
    }

    public async Task<List<TopSellerDto>> GetTopSellersAsync(DateTime startDate, DateTime endDate, int top = 10)
    {
        // Ensure end date includes the entire day
        var adjustedEndDate = endDate.Date.AddDays(1).AddTicks(-1);
        
        var topSellers = await (from order in _context.Orders
                                join user in _context.Users on order.SellerId equals user.Id
                                where order.CreatedAt >= startDate && order.CreatedAt <= adjustedEndDate
                                      && order.Status == "delivered"  // Only include delivered orders
                                group new { order, user } by new { user.Id, user.FullName } into g
                                select new TopSellerDto
                                {
                                    SellerId = g.Key.Id,
                                    SellerName = g.Key.FullName,
                                    Revenue = g.Sum(x => x.order.TotalAmount),
                                    OrderCount = g.Count(),
                                    Commission = g.Sum(x => x.order.TotalAmount) * COMMISSION_RATE
                                })
                                .OrderByDescending(s => s.Revenue)
                                .Take(top)
                                .ToListAsync();

        return topSellers;
    }

    public async Task<(decimal Revenue, decimal Commission, int OrderCount)> GetOverallStatsAsync(DateTime startDate, DateTime endDate)
    {
        // Ensure end date includes the entire day
        var adjustedEndDate = endDate.Date.AddDays(1).AddTicks(-1);
        
        // Debug: Check all orders first
        var allOrders = await _context.Orders.ToListAsync();
        var ordersInRange = allOrders.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= adjustedEndDate).ToList();
        var deliveredOrders = ordersInRange.Where(o => o.Status == "delivered").ToList();
        
        Console.WriteLine($"[GetOverallStatsAsync] Total orders in DB: {allOrders.Count}");
        Console.WriteLine($"[GetOverallStatsAsync] Orders in date range: {ordersInRange.Count}");
        Console.WriteLine($"[GetOverallStatsAsync] Delivered orders in date range: {deliveredOrders.Count}");
        Console.WriteLine($"[GetOverallStatsAsync] Date range: {startDate:dd/MM/yyyy HH:mm:ss} to {adjustedEndDate:dd/MM/yyyy HH:mm:ss}");
        
        // Debug: Show some order dates and statuses
        foreach (var order in allOrders.Take(5))
        {
            Console.WriteLine($"[GetOverallStatsAsync] Order {order.Id}: CreatedAt={order.CreatedAt:dd/MM/yyyy HH:mm:ss}, Status={order.Status}");
        }
        
        var stats = await (from order in _context.Orders
                           where order.CreatedAt >= startDate && order.CreatedAt <= adjustedEndDate
                           && order.Status == "delivered"  // Only include delivered orders
                           group order by 1 into g
                           select new
                           {
                               Revenue = g.Sum(o => o.TotalAmount),
                               OrderCount = g.Count()
                           })
                           .FirstOrDefaultAsync();

        var revenue = stats?.Revenue ?? 0;
        var commission = revenue * COMMISSION_RATE;
        var orderCount = stats?.OrderCount ?? 0;
        
        Console.WriteLine($"[GetOverallStatsAsync] Results: Revenue={revenue}, Orders={orderCount}");

        return (revenue, commission, orderCount);
    }
}
