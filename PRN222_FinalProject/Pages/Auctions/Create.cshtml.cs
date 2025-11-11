using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Auctions;

public class CreateModel : PageModel
{
    private readonly IAuctionService _auctionService;
    private readonly IProductService _productService;

    public CreateModel(IAuctionService auctionService, IProductService productService)
    {
        _auctionService = auctionService;
        _productService = productService;
    }

    [BindProperty]
    public CreateAuctionDto CreateDto { get; set; } = new CreateAuctionDto();

    public IEnumerable<ProductDto> MyProducts { get; set; } = new List<ProductDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Get seller's active products
        MyProducts = await _productService.GetProductsBySellerIdAsync(currentUser.Id);
        MyProducts = MyProducts.Where(p => p.IsActive == true);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        MyProducts = await _productService.GetProductsBySellerIdAsync(currentUser.Id);
        MyProducts = MyProducts.Where(p => p.IsActive == true);


        var result = await _auctionService.CreateAuctionAsync(currentUser.Id, CreateDto);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Auctions/MyAuctions");
        }

        ModelState.AddModelError(string.Empty, result.Message);
        return Page();
    }
}
