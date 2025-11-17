using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Staff.Products;

public class PendingModel : PageModel
{
    private readonly IProductService _productService;

    public PendingModel(IProductService productService)
    {
        _productService = productService;
    }

    public IEnumerable<ProductDto> PendingProducts { get; set; } = new List<ProductDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        // Check staff permission
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || (currentUser.Role != "admin" && currentUser.Role != "staff"))
        {
            return RedirectToPage("/Account/Login");
        }

        PendingProducts = await _productService.GetPendingProductsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int productId)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || (currentUser.Role != "admin" && currentUser.Role != "staff"))
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _productService.ApproveProductAsync(productId, currentUser.Id);
        
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Đã duyệt sản phẩm thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int productId, string reason)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || (currentUser.Role != "admin" && currentUser.Role != "staff"))
        {
            return RedirectToPage("/Account/Login");
        }
        
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập lý do từ chối!";
            return RedirectToPage();
        }

        var result = await _productService.RejectProductAsync(productId, currentUser.Id, reason);
        
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Đã từ chối sản phẩm!";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage();
    }
}
