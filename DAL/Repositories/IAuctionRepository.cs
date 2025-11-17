using DAL.Entities;

namespace DAL.Repositories;

public interface IAuctionRepository
{
    Task<Auction?> GetByIdAsync(int id);
    Task<Auction?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Auction>> GetAllAsync();
    Task<IEnumerable<Auction>> GetActiveAuctionsAsync();
    Task<IEnumerable<Auction>> GetPendingAuctionsAsync();
    Task<IEnumerable<Auction>> GetBySellerIdAsync(int sellerId);
    Task<IEnumerable<Auction>> GetByProductIdAsync(int productId);
    Task<(IEnumerable<Auction> Auctions, int TotalCount)> GetWithPaginationAsync(int pageNumber, int pageSize, string? status = null);
    Task<Auction> CreateAsync(Auction auction);
    Task<Auction> UpdateAsync(Auction auction);
    Task<bool> CloseAuctionAsync(int auctionId, int winnerId);
    Task<bool> CancelAuctionAsync(int auctionId);
    Task<IEnumerable<Auction>> GetExpiredAuctionsAsync();
}
