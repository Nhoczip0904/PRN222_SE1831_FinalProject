using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly EvBatteryTrading2Context _context;

    public AuctionRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<Auction?> GetByIdAsync(int id)
    {
        return await _context.Auctions
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Auction?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Auctions
            .Include(a => a.Product)
            .Include(a => a.Seller)
            .Include(a => a.Winner)
            .Include(a => a.Bids)
                .ThenInclude(b => b.Bidder)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Auction>> GetAllAsync()
    {
        return await _context.Auctions
            .Include(a => a.Product)
            .Include(a => a.Seller)
            .Include(a => a.Winner)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Auction>> GetActiveAuctionsAsync()
    {
        return await _context.Auctions
            .Include(a => a.Product)
            .Include(a => a.Seller)
            .Where(a => a.Status == "active" && a.EndTime > DateTime.Now)
            .OrderBy(a => a.EndTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Auction>> GetBySellerIdAsync(int sellerId)
    {
        return await _context.Auctions
            .Include(a => a.Product)
            .Include(a => a.Winner)
            .Where(a => a.SellerId == sellerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Auction>> GetByProductIdAsync(int productId)
    {
        return await _context.Auctions
            .Include(a => a.Seller)
            .Where(a => a.ProductId == productId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Auction> Auctions, int TotalCount)> GetWithPaginationAsync(int pageNumber, int pageSize, string? status = null)
    {
        var query = _context.Auctions
            .Include(a => a.Product)
            .Include(a => a.Seller)
            .Include(a => a.Winner)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }

        var totalCount = await query.CountAsync();
        var auctions = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (auctions, totalCount);
    }

    public async Task<Auction> CreateAsync(Auction auction)
    {
        auction.CreatedAt = DateTime.Now;
        auction.UpdatedAt = DateTime.Now;
        auction.Status = "active";
        auction.CurrentPrice = auction.StartingPrice;

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();
        return auction;
    }

    public async Task<Auction> UpdateAsync(Auction auction)
    {
        auction.UpdatedAt = DateTime.Now;

        _context.Auctions.Update(auction);
        await _context.SaveChangesAsync();
        return auction;
    }

    public async Task<bool> CloseAuctionAsync(int auctionId, int winnerId)
    {
        var auction = await GetByIdAsync(auctionId);
        if (auction == null)
            return false;

        auction.Status = "closed";
        auction.WinnerId = winnerId;
        auction.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelAuctionAsync(int auctionId)
    {
        var auction = await GetByIdAsync(auctionId);
        if (auction == null)
            return false;

        auction.Status = "cancelled";
        auction.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Auction>> GetExpiredAuctionsAsync()
    {
        return await _context.Auctions
            .Include(a => a.Bids)
                .ThenInclude(b => b.Bidder)
            .Where(a => a.Status == "active" && a.EndTime <= DateTime.Now)
            .ToListAsync();
    }
}
