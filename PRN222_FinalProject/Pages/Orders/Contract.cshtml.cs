using BLL.Helpers;
using BLL.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;

namespace PRN222_FinalProject.Pages.Orders;

public class ContractModel : PageModel
{
    private readonly EvBatteryTrading2Context _context;

    public ContractModel(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public Order? Order { get; set; }
    public User? Seller { get; set; }
    public User? Buyer { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int orderId)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Order = await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (Order == null)
        {
            return NotFound();
        }

        // Check permission
        if (Order.BuyerId != currentUser.Id && Order.SellerId != currentUser.Id)
        {
            return Forbid();
        }

        OrderItems = await _context.OrderItems
            .Include(oi => oi.Product)
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();

        Seller = Order.Seller;
        Buyer = Order.Buyer;

        return Page();
    }
}
