using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Products;

public class MyProductsModel : PageModel
{
    private readonly IProductService _productService;

    public MyProductsModel(IProductService productService)
    {
        _productService = productService;
    }

    public IEnumerable<ProductDto>? Products { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Products = await _productService.GetProductsBySellerIdAsync(currentUser.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _productService.DeactivateProductAsync(id, currentUser.Id);

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

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _productService.DeleteProductAsync(id, currentUser.Id);

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
