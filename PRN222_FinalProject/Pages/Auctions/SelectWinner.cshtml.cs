using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PRN222_FinalProject.Hubs;
using PRN222_FinalProject.Services;
using Microsoft.EntityFrameworkCore;

namespace PRN222_FinalProject.Pages.Auctions;

public class SelectWinnerModel : PageModel
{
    private readonly IAuctionService _auctionService;
    private readonly IBidService _bidService;
    INotificationService _notificationService; // Add to constructor

    public SelectWinnerModel(IAuctionService auctionService, IBidService bidService, INotificationService notificationService)
    {
        _auctionService = auctionService;
        _bidService = bidService;
        _notificationService = notificationService;
    }

    public AuctionDto? Auction { get; set; }
    public IEnumerable<BidDto>? Bids { get; set; }

    public async Task<IActionResult> OnGetAsync(int auctionId)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Auction = await _auctionService.GetAuctionByIdAsync(auctionId);
        
        if (Auction == null)
        {
            TempData["ErrorMessage"] = "Đấu giá không tồn tại";
            return RedirectToPage("/Auctions/MyAuctions");
        }

        // Check if user is the seller
        if (Auction.SellerId != currentUser.Id)
        {
            TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này";
            return RedirectToPage("/Auctions/MyAuctions");
        }

        Bids = await _bidService.GetBidsByAuctionIdAsync(auctionId);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int auctionId, int winnerId)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _auctionService.SelectWinnerAsync(auctionId, currentUser.Id, winnerId);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;

            // Lấy connectionId của user
            var auction = await _auctionService.GetAuctionByIdAsync(auctionId);
            var winningBid = (await _bidService.GetWinningBidAsync(auctionId))?.BidAmount ?? 0;
            if (auction != null)
            {
                await _notificationService.NotifyAuctionWinnerAsync(
                    winnerId,
                    auction.Id,
                    auction.ProductName,
                    winningBid
                );
            }

            return RedirectToPage("/Auctions/MyAuctions");
        }

        TempData["ErrorMessage"] = result.Message;
        return RedirectToPage("/Auctions/SelectWinner", new { auctionId });
    }

    public async Task<IActionResult> OnPostCloseWithHighestAsync(int auctionId)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _auctionService.CloseAuctionWithHighestBidderAsync(auctionId, currentUser.Id);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Auctions/MyAuctions");
        }

        TempData["ErrorMessage"] = result.Message;
        return RedirectToPage("/Auctions/SelectWinner", new { auctionId });
    }
}
