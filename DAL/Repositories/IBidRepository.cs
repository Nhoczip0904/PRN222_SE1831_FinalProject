using DAL.Entities;

namespace DAL.Repositories;

public interface IBidRepository
{
    Task<Bid?> GetByIdAsync(int id);
    Task<IEnumerable<Bid>> GetByAuctionIdAsync(int auctionId);
    Task<IEnumerable<Bid>> GetByBidderIdAsync(int bidderId);
    Task<Bid?> GetHighestBidAsync(int auctionId);
    Task<Bid?> GetWinningBidAsync(int auctionId);
    Task<Bid> CreateAsync(Bid bid);
    Task<int> GetBidCountAsync(int auctionId);
}
