using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Orders;

public class IndexModel : PageModel
{
    private readonly IOrderService _orderService;
    private readonly IContractService _contractService;
    private readonly IDeliveryService _deliveryService;

    public IndexModel(IOrderService orderService, IContractService contractService, IDeliveryService deliveryService)
    {
        _orderService = orderService;
        _contractService = contractService;
        _deliveryService = deliveryService;
    }

    public IEnumerable<OrderDto>? Orders { get; set; }
    public Dictionary<int, Contract?> OrderContracts { get; set; } = new();
    public string ViewType { get; set; } = "buyer"; // buyer or seller
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }

    public async Task<IActionResult> OnGetAsync(string? viewType, int pageNumber = 1)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        ViewType = viewType ?? "buyer";
        PageNumber = pageNumber;

        if (ViewType == "seller")
        {
            var result = await _orderService.GetOrdersBySellerIdWithPaginationAsync(currentUser.Id, pageNumber, 10);
            Orders = result.Orders;
            TotalPages = result.TotalPages;
        }
        else
        {
            var result = await _orderService.GetOrdersByBuyerIdWithPaginationAsync(currentUser.Id, pageNumber, 10);
            Orders = result.Orders;
            TotalPages = result.TotalPages;
        }

        // Load contract info for each order
        if (Orders != null)
        {
            foreach (var order in Orders)
            {
                var contract = await _contractService.GetContractByOrderIdAsync(order.Id);
                OrderContracts[order.Id] = contract;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _orderService.CancelOrderAsync(id, currentUser.Id);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _orderService.UpdateOrderStatusAsync(id, status, currentUser.Id, currentUser.Role ?? "member");

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostConfirmDeliveryAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _deliveryService.ConfirmDeliveryAsync(id, currentUser.Id, "Đã nhận hàng");

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage();
    }
}
