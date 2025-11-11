using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Cart;

public class IndexModel : PageModel
{
    private readonly ICartService _cartService;

    public IndexModel(ICartService cartService)
    {
        _cartService = cartService;
    }

    public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();

    public IActionResult OnGet()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        CartItems = _cartService.GetCart("ShoppingCart");

        return Page();
    }

    public IActionResult OnPostUpdateQuantity(int productId, int quantity)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = _cartService.UpdateCartItem("ShoppingCart", productId, quantity);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToPage();
    }

    public IActionResult OnPostRemove(int productId)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = _cartService.RemoveFromCart("ShoppingCart", productId);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToPage();
    }
}
