using BLL.DTOs;

namespace BLL.Services;

public interface IRevenueService
{
    Task<RevenueDashboardDto> GetRevenueDashboardAsync(RevenueSearchDto searchDto);
    Task<List<CategoryRevenueDto>> GetCategoryRevenuesAsync(DateTime startDate, DateTime endDate);
    Task<List<DailyRevenueDto>> GetDailyRevenuesAsync(DateTime startDate, DateTime endDate, string groupBy = "day");
    Task<List<TopProductDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int top = 10);
    Task<List<TopSellerDto>> GetTopSellersAsync(DateTime startDate, DateTime endDate, int top = 10);
    Task<(decimal Revenue, decimal Commission, int OrderCount)> GetOverallStatsAsync(DateTime startDate, DateTime endDate);
}
