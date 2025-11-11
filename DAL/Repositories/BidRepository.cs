using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class BidRepository : IBidRepository
{
    private readonly EvBatteryTrading2Context _context;

    public BidRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<Bid?> GetByIdAsync(int id)
    {
        return await _context.Bids
            .Include(b => b.Bidder)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Bid>> GetByAuctionIdAsync(int auctionId)
    {
        return await _context.Bids
            .Include(b => b.Bidder)
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bid>> GetByBidderIdAsync(int bidderId)
    {
        return await _context.Bids
            .Include(b => b.Auction)
                .ThenInclude(a => a.Product)
            .Where(b => b.BidderId == bidderId)
            .OrderByDescending(b => b.BidTime)
            .ToListAsync();
    }

    public async Task<Bid?> GetHighestBidAsync(int auctionId)
    {
        return await _context.Bids
            .Include(b => b.Bidder)
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidAmount)
            .FirstOrDefaultAsync();
    }

    public async Task<Bid?> GetWinningBidAsync(int auctionId)
    {
        return await _context.Bids
            .Include(b => b.Bidder)
            .FirstOrDefaultAsync(b => b.AuctionId == auctionId && b.IsWinning);
    }

    public async Task<Bid> CreateAsync(Bid bid)
    {
        bid.BidTime = DateTime.Now;
        bid.IsWinning = false; // Will be updated by trigger

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();
        return bid;
    }

    public async Task<int> GetBidCountAsync(int auctionId)
    {
        return await _context.Bids
            .Where(b => b.AuctionId == auctionId)
            .CountAsync();
    }
}
