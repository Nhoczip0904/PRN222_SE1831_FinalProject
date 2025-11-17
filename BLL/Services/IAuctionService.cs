using BLL.DTOs;

namespace BLL.Services;

public interface IAuctionService
{
    Task<AuctionDto?> GetAuctionByIdAsync(int id);
    Task<IEnumerable<AuctionDto>> GetAllAuctionsAsync();
    Task<IEnumerable<AuctionDto>> GetActiveAuctionsAsync();
    Task<IEnumerable<AuctionDto>> GetPendingAuctionsAsync();
    Task<IEnumerable<AuctionDto>> GetAuctionsBySellerIdAsync(int sellerId);
    Task<(IEnumerable<AuctionDto> Auctions, int TotalCount, int TotalPages)> GetAuctionsWithPaginationAsync(int pageNumber, int pageSize, string? status = null);
    Task<(bool Success, string Message, int? AuctionId)> CreateAuctionAsync(int sellerId, CreateAuctionDto createDto);
    Task<(bool Success, string Message)> CloseAuctionAsync(int auctionId, int adminId);
    Task<(bool Success, string Message)> SelectWinnerAsync(int auctionId, int sellerId, int winnerId);
    Task<(bool Success, string Message)> CloseAuctionWithHighestBidderAsync(int auctionId, int sellerId);
    Task<(bool Success, string Message)> CancelAuctionAsync(int auctionId, int sellerId);
    Task<(bool Success, string Message)> ApproveAuctionAsync(int auctionId, int approvedById);
    Task<(bool Success, string Message)> RejectAuctionAsync(int auctionId, int approvedById, string reason);
    Task ProcessExpiredAuctionsAsync();
}
