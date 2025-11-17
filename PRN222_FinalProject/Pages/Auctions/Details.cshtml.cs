using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_FinalProject.Services; // Add this for INotificationService

namespace PRN222_FinalProject.Pages.Auctions;

public class DetailsModel : PageModel
{
    private readonly IAuctionService _auctionService;
    private readonly IBidService _bidService;
    private readonly INotificationService _notificationService; // Use the notification service

    public DetailsModel(
        IAuctionService auctionService,
        IBidService bidService,
        INotificationService notificationService // Inject here
    )
    {
        _auctionService = auctionService;
        _bidService = bidService;
        _notificationService = notificationService;
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

    public async Task<PartialViewResult> OnGetBidsListAsync(int id)
    {
        var bids = await _bidService.GetBidsByAuctionIdAsync(id);
        return Partial("_BidsList", bids);
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

            // Lấy auction
            var auction = await _auctionService.GetAuctionByIdAsync(PlaceBidDto.AuctionId);

            // ❗️ KIỂM TRA NULL BẮT BUỘC ❗️
            if (auction == null)
            {
                // Ghi log lỗi nếu cần
                // _logger.LogError($"Không tìm thấy auction {PlaceBidDto.AuctionId} để gửi thông báo.");
            }
            else
            {
                // Chỉ gửi thông báo nếu auction tồn tại
                await _notificationService.NotifyNewBidAsync(
                    auction.SellerId,
                    auction.Id,
                    PlaceBidDto.BidAmount
                );
            }
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage("/Auctions/Details", new { id = PlaceBidDto.AuctionId });
    }
}
