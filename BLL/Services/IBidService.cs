using BLL.DTOs;

namespace BLL.Services;

public interface IBidService
{
    Task<IEnumerable<BidDto>> GetBidsByAuctionIdAsync(int auctionId);
    Task<IEnumerable<BidDto>> GetBidsByBidderIdAsync(int bidderId);
    Task<BidDto?> GetWinningBidAsync(int auctionId);
    Task<(bool Success, string Message)> PlaceBidAsync(int bidderId, PlaceBidDto placeBidDto);
}
