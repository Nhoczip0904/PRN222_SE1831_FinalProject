using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Admin.Orders;

public class IndexModel : PageModel
{
    private readonly IOrderService _orderService;

    public IndexModel(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public List<OrderDto> Orders { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public string? StatusFilter { get; set; }
    public string? Keyword { get; set; }

    // Statistics
    public int PendingCount { get; set; }
    public int ShippedCount { get; set; }
    public int DeliveredCount { get; set; }
    public int CancelledCount { get; set; }

    public async Task<IActionResult> OnGetAsync(string? status, string? keyword, int pageNumber = 1)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        StatusFilter = status;
        Keyword = keyword;
        PageNumber = pageNumber;

        // Get all orders with pagination
        var allOrders = await _orderService.GetAllOrdersAsync();
        
        // Filter by status
        var filteredOrders = allOrders.AsEnumerable();
        if (!string.IsNullOrEmpty(status))
        {
            filteredOrders = filteredOrders.Where(o => o.Status == status);
        }

        // Filter by keyword
        if (!string.IsNullOrEmpty(keyword))
        {
            filteredOrders = filteredOrders.Where(o =>
                o.Id.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (o.BuyerName != null && o.BuyerName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (o.SellerName != null && o.SellerName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            );
        }

        // Calculate statistics
        PendingCount = allOrders.Count(o => o.Status == "pending");
        ShippedCount = allOrders.Count(o => o.Status == "shipped");
        DeliveredCount = allOrders.Count(o => o.Status == "delivered");
        CancelledCount = allOrders.Count(o => o.Status == "cancelled");

        // Pagination
        TotalCount = filteredOrders.Count();
        TotalPages = (int)Math.Ceiling(TotalCount / 20.0);
        Orders = filteredOrders
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * 20)
            .Take(20)
            .ToList();

        return Page();
    }
}
