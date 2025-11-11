using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Auctions;

public class MyAuctionsModel : PageModel
{
    private readonly IAuctionService _auctionService;

    public MyAuctionsModel(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    public IEnumerable<AuctionDto>? Auctions { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Auctions = await _auctionService.GetAuctionsBySellerIdAsync(currentUser.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _auctionService.CancelAuctionAsync(id, currentUser.Id);

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
