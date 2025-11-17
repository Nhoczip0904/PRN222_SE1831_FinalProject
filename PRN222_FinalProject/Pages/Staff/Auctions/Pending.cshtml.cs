using BLL.Helpers;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Staff.Auctions
{
    public class PendingModel : PageModel
    {
        private readonly IAuctionService _auctionService;

        public PendingModel(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        public IEnumerable<AuctionDto> PendingAuctions { get; set; } = new List<AuctionDto>();

        [TempData]
        public string Message { get; set; } = "";

        [TempData]
        public bool Success { get; set; } = true;

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null || (currentUser.Role != "staff" && currentUser.Role != "admin"))
            {
                return RedirectToPage("/Account/Login");
            }

            PendingAuctions = await _auctionService.GetPendingAuctionsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int auctionId)
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null || (currentUser.Role != "staff" && currentUser.Role != "admin"))
            {
                return RedirectToPage("/Account/Login");
            }

            var result = await _auctionService.ApproveAuctionAsync(auctionId, currentUser.Id);
            
            Message = result.Message;
            Success = result.Success;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int auctionId, string reason)
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null || (currentUser.Role != "staff" && currentUser.Role != "admin"))
            {
                return RedirectToPage("/Account/Login");
            }

            var result = await _auctionService.RejectAuctionAsync(auctionId, currentUser.Id, reason);
            
            Message = result.Message;
            Success = result.Success;

            return RedirectToPage();
        }
    }
}
