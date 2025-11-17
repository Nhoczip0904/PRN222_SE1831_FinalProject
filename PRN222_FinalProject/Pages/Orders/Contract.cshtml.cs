using BLL.Helpers;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Orders;

public class ContractModel : PageModel
{
    private readonly IOrderService _orderService;

    public ContractModel(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public ContractDto? Contract { get; set; }

    public async Task<IActionResult> OnGetAsync(int orderId)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Contract = await _orderService.GetContractDetailsAsync(orderId);

        if (Contract == null)
        {
            return NotFound();
        }

        // Check permission
        if (Contract.BuyerId != currentUser.Id && Contract.SellerId != currentUser.Id)
        {
            return Forbid();
        }

        return Page();
    }
}
