using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Admin.Orders;

public class DetailsModel : PageModel
{
    private readonly IOrderService _orderService;

    public DetailsModel(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public OrderDto Order { get; set; } = null!;
    public string? Message { get; set; }
    public bool Success { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        Order = order;
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, string newStatus)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
        
        Success = result.Success;
        Message = result.Message;

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        Order = order;
        return Page();
    }
}
