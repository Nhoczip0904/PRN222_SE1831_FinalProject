using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Products;

public class DetailsAdminModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ICartService _cartService;

    public DetailsAdminModel(IProductService productService, ICartService cartService)
    {
        _productService = productService;
        _cartService = cartService;
    }

    public ProductDto? Product { get; set; }
    public UserDto? CurrentUser { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        CurrentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        Product = await _productService.GetProductByIdAsync(id);

        if (Product == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(int productId)
    {
        CurrentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");

        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = _cartService.AddToCart("ShoppingCart", productId, 1);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage("/Products/Details", new { id = productId });
    }
}
