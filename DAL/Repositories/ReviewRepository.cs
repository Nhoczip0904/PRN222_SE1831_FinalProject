using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly EvBatteryTrading2Context _context;

    public ReviewRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews
            .Include(r => r.Order)
            .Include(r => r.Product)
            .Include(r => r.Buyer)
            .Include(r => r.Seller)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Review?> GetByOrderIdAsync(int orderId)
    {
        return await _context.Reviews
            .Include(r => r.Order)
            .Include(r => r.Product)
            .Include(r => r.Buyer)
            .Include(r => r.Seller)
            .FirstOrDefaultAsync(r => r.OrderId == orderId);
    }

    public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
    {
        return await _context.Reviews
            .Where(r => r.ProductId == productId)
            .Include(r => r.Buyer)
            .Include(r => r.Seller)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetBySellerIdAsync(int sellerId)
    {
        return await _context.Reviews
            .Where(r => r.SellerId == sellerId)
            .Include(r => r.Buyer)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByBuyerIdAsync(int buyerId)
    {
        return await _context.Reviews
            .Where(r => r.BuyerId == buyerId)
            .Include(r => r.Product)
            .Include(r => r.Seller)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsByOrderIdAsync(int orderId)
    {
        return await _context.Reviews.AnyAsync(r => r.OrderId == orderId);
    }

    public async Task<Review> CreateAsync(Review review)
    {
        review.CreatedAt = DateTime.Now;
        review.UpdatedAt = DateTime.Now;
        
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        
        return review;
    }

    public async Task<Review> UpdateAsync(Review review)
    {
        review.UpdatedAt = DateTime.Now;
        
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
        
        return review;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
            return false;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(decimal AverageRating, int TotalReviews)> GetProductRatingStatsAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        if (!reviews.Any())
        {
            return (0, 0);
        }

        var averageRating = reviews.Average(r => r.ProductRating);
        return (Math.Round((decimal)averageRating, 2), reviews.Count);
    }

    public async Task<SellerRating?> GetSellerRatingAsync(int sellerId)
    {
        return await _context.SellerRatings
            .FirstOrDefaultAsync(sr => sr.SellerId == sellerId);
    }
}
