using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Staff.Auctions;

public class IndexModel : PageModel
{
    private readonly IAuctionService _auctionService;

    public IndexModel(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    public IEnumerable<AuctionDto>? Auctions { get; set; }
    public string Status { get; set; } = "active";
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }

    public async Task<IActionResult> OnGetAsync(string? status, int pageNumber = 1)
    {
        // Check staff permission
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || (currentUser.Role != "admin" && currentUser.Role != "staff"))
        {
            return RedirectToPage("/Account/Login");
        }

        Status = status ?? "active";
        PageNumber = pageNumber;

        if (Status == "expired")
        {
            // Get all active auctions and filter expired ones
            var allAuctions = await _auctionService.GetAllAuctionsAsync();
            Auctions = allAuctions.Where(a => a.Status == "active" && a.ApprovalStatus == "approved" && a.IsExpired);
            TotalPages = 1;
        }
        else
        {
            var result = await _auctionService.GetAuctionsWithPaginationAsync(pageNumber, 20, Status);
            Auctions = result.Auctions;
            TotalPages = result.TotalPages;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCloseAsync(int id)
    {
        // Check staff permission
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || (currentUser.Role != "admin" && currentUser.Role != "staff"))
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _auctionService.CloseAuctionAsync(id, currentUser.Id);

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
