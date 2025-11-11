using DAL.Entities;

namespace DAL.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id);
    Task<Review?> GetByOrderIdAsync(int orderId);
    Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
    Task<IEnumerable<Review>> GetBySellerIdAsync(int sellerId);
    Task<IEnumerable<Review>> GetByBuyerIdAsync(int buyerId);
    Task<bool> ExistsByOrderIdAsync(int orderId);
    Task<Review> CreateAsync(Review review);
    Task<Review> UpdateAsync(Review review);
    Task<bool> DeleteAsync(int id);
    Task<(decimal AverageRating, int TotalReviews)> GetProductRatingStatsAsync(int productId);
    Task<SellerRating?> GetSellerRatingAsync(int sellerId);
}
