using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;

namespace PRN222_FinalProject.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IOrderService _orderService;
    private readonly EvBatteryTrading2Context _context;

    public DashboardModel(IUserService userService, IOrderService orderService, EvBatteryTrading2Context context)
    {
        _userService = userService;
        _orderService = orderService;
        _context = context;
    }

    public int TotalUsers { get; set; }
    public int VerifiedUsers { get; set; }
    public int UnverifiedUsers { get; set; }
    public int AdminUsers { get; set; }
    
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        var allUsers = await _userService.GetAllUsersAsync();
        
        TotalUsers = allUsers.Count();
        VerifiedUsers = allUsers.Count(u => u.IsVerified == true);
        UnverifiedUsers = allUsers.Count(u => u.IsVerified == false);
        AdminUsers = allUsers.Count(u => u.Role == "admin");

        // Get order statistics
        var allOrders = await _orderService.GetAllOrdersAsync();
        TotalOrders = allOrders.Count();
        TotalRevenue = allOrders.Sum(o => o.TotalAmount);

        // Get commission revenue (25% of all orders)
        var revenues = await _context.SystemRevenues.ToListAsync();
        TotalCommission = revenues.Sum(r => r.CommissionAmount);

        return Page();
    }
}
