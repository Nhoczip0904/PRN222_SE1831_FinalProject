using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Auctions;

public class IndexModel : PageModel
{
    private readonly IAuctionService _auctionService;

    public IndexModel(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    public IEnumerable<AuctionDto>? Auctions { get; set; }
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }

    public async Task OnGetAsync(int pageNumber = 1)
    {
        PageNumber = pageNumber;

        var result = await _auctionService.GetAuctionsWithPaginationAsync(pageNumber, 9, "active");
        Auctions = result.Auctions;
        TotalPages = result.TotalPages;
    }
}
