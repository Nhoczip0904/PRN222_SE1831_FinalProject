using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IOrderService _orderService;
    private readonly IAuctionService _auctionService;

    public DashboardModel(IUserService userService, IOrderService orderService, IAuctionService auctionService)
    {
        _userService = userService;
        _orderService = orderService;
        _auctionService = auctionService;
    }

    // User Statistics
    public int TotalUsers { get; set; }
    public int VerifiedUsers { get; set; }
    public int UnverifiedUsers { get; set; }
    public int AdminUsers { get; set; }
    
    // Order Statistics
    public int TotalOrders { get; set; }
    
    // Auction Statistics
    public int AuctionsPendingApproval { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        // Get user statistics
        var allUsers = await _userService.GetAllUsersAsync();
        TotalUsers = allUsers.Count();
        VerifiedUsers = allUsers.Count(u => u.IsVerified == true);
        UnverifiedUsers = allUsers.Count(u => u.IsVerified == false);
        AdminUsers = allUsers.Count(u => u.Role == "admin");

        // Get order statistics
        var orders = await _orderService.GetAllOrdersAsync();
        TotalOrders = orders.Count();
        
        // Get auction statistics
        AuctionsPendingApproval = (await _auctionService.GetPendingAuctionsAsync()).Count();

        return Page();
    }
}
