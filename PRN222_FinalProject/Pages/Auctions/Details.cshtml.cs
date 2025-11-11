using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Auctions;

public class DetailsModel : PageModel
{
    private readonly IAuctionService _auctionService;
    private readonly IBidService _bidService;

    public DetailsModel(IAuctionService auctionService, IBidService bidService)
    {
        _auctionService = auctionService;
        _bidService = bidService;
    }

    public AuctionDto? Auction { get; set; }
    public IEnumerable<BidDto>? Bids { get; set; }
    public UserDto? CurrentUser { get; set; }

    [BindProperty]
    public PlaceBidDto PlaceBidDto { get; set; } = new PlaceBidDto();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        CurrentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");

        Auction = await _auctionService.GetAuctionByIdAsync(id);
        if (Auction == null)
        {
            return NotFound();
        }

        Bids = await _bidService.GetBidsByAuctionIdAsync(id);

        return Page();
    }

    public async Task<IActionResult> OnPostPlaceBidAsync()
    {
        CurrentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");

        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        if (!ModelState.IsValid)
        {
            Auction = await _auctionService.GetAuctionByIdAsync(PlaceBidDto.AuctionId);
            Bids = await _bidService.GetBidsByAuctionIdAsync(PlaceBidDto.AuctionId);
            return Page();
        }

        var result = await _bidService.PlaceBidAsync(CurrentUser.Id, PlaceBidDto);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage("/Auctions/Details", new { id = PlaceBidDto.AuctionId });
    }
}
