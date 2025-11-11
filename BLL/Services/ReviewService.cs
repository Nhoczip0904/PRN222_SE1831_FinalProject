using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services;

public class CreateReviewDto
{
    public int OrderId { get; set; }
    public int ProductRating { get; set; } // 1-5
    public string? ProductReview { get; set; }
    public int SellerRating { get; set; } // 1-5
    public string? SellerReview { get; set; }
    public string? Images { get; set; } // JSON array
}

public class UpdateReviewDto
{
    public int ProductRating { get; set; }
    public string? ProductReview { get; set; }
    public int SellerRating { get; set; }
    public string? SellerReview { get; set; }
    public string? Images { get; set; }
}

public interface IReviewService
{
    Task<(bool Success, string Message, Review? Review)> CreateReviewAsync(int buyerId, CreateReviewDto dto);
    Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, int buyerId, UpdateReviewDto dto);
    Task<(bool Success, string Message)> AddSellerResponseAsync(int reviewId, int sellerId, string response);
    Task<List<Review>> GetProductReviewsAsync(int productId);
    Task<List<Review>> GetSellerReviewsAsync(int sellerId);
    Task<Review?> GetOrderReviewAsync(int orderId);
    Task<SellerRating?> GetSellerRatingAsync(int sellerId);
    Task<bool> CanReviewAsync(int orderId, int buyerId);
    Task<(decimal AverageRating, int TotalReviews)> GetProductRatingStatsAsync(int productId);
}

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;

    public ReviewService(IReviewRepository reviewRepository, IOrderRepository orderRepository)
    {
        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
    }

    public async Task<(bool Success, string Message, Review? Review)> CreateReviewAsync(int buyerId, CreateReviewDto dto)
    {
        // Validate order
        var order = await _orderRepository.GetByIdWithDetailsAsync(dto.OrderId);

        if (order == null)
        {
            return (false, "Đơn hàng không tồn tại", null);
        }

        if (order.BuyerId != buyerId)
        {
            return (false, "Bạn không có quyền đánh giá đơn hàng này", null);
        }

        if (order.Status != "delivered")
        {
            return (false, "Chỉ có thể đánh giá sau khi đã nhận hàng", null);
        }

        // Check if already reviewed
        var existingReview = await _reviewRepository.ExistsByOrderIdAsync(dto.OrderId);

        if (existingReview)
        {
            return (false, "Bạn đã đánh giá đơn hàng này rồi", null);
        }

        // Validate ratings
        if (dto.ProductRating < 1 || dto.ProductRating > 5 || dto.SellerRating < 1 || dto.SellerRating > 5)
        {
            return (false, "Đánh giá phải từ 1 đến 5 sao", null);
        }

        // Get product ID from order
        var productId = order.OrderItems.FirstOrDefault()?.ProductId;
        if (productId == null)
        {
            return (false, "Không tìm thấy sản phẩm trong đơn hàng", null);
        }

        // Create review
        var review = new Review
        {
            OrderId = dto.OrderId,
            ProductId = productId.Value,
            BuyerId = buyerId,
            SellerId = order.SellerId ?? 0,
            ProductRating = dto.ProductRating,
            ProductReview = dto.ProductReview,
            SellerRating = dto.SellerRating,
            SellerReview = dto.SellerReview,
            Images = dto.Images,
            IsVerified = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _reviewRepository.CreateAsync(review);

        // Trigger will automatically update seller_ratings table

        return (true, "Đánh giá thành công. Cảm ơn bạn đã chia sẻ!", review);
    }

    public async Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, int buyerId, UpdateReviewDto dto)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);

        if (review == null)
        {
            return (false, "Đánh giá không tồn tại");
        }

        if (review.BuyerId != buyerId)
        {
            return (false, "Bạn không có quyền chỉnh sửa đánh giá này");
        }

        // Validate ratings
        if (dto.ProductRating < 1 || dto.ProductRating > 5 || dto.SellerRating < 1 || dto.SellerRating > 5)
        {
            return (false, "Đánh giá phải từ 1 đến 5 sao");
        }

        review.ProductRating = dto.ProductRating;
        review.ProductReview = dto.ProductReview;
        review.SellerRating = dto.SellerRating;
        review.SellerReview = dto.SellerReview;
        review.Images = dto.Images;

        await _reviewRepository.UpdateAsync(review);

        return (true, "Cập nhật đánh giá thành công");
    }

    public async Task<(bool Success, string Message)> AddSellerResponseAsync(int reviewId, int sellerId, string response)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);

        if (review == null)
        {
            return (false, "Đánh giá không tồn tại");
        }

        if (review.SellerId != sellerId)
        {
            return (false, "Bạn không có quyền phản hồi đánh giá này");
        }

        if (string.IsNullOrWhiteSpace(response))
        {
            return (false, "Nội dung phản hồi không được để trống");
        }

        review.SellerResponse = response;
        review.SellerResponseAt = DateTime.Now;

        await _reviewRepository.UpdateAsync(review);

        return (true, "Phản hồi đánh giá thành công");
    }

    public async Task<List<Review>> GetProductReviewsAsync(int productId)
    {
        var reviews = await _reviewRepository.GetByProductIdAsync(productId);
        return reviews.ToList();
    }

    public async Task<List<Review>> GetSellerReviewsAsync(int sellerId)
    {
        var reviews = await _reviewRepository.GetBySellerIdAsync(sellerId);
        return reviews.ToList();
    }

    public async Task<Review?> GetOrderReviewAsync(int orderId)
    {
        return await _reviewRepository.GetByOrderIdAsync(orderId);
    }

    public async Task<SellerRating?> GetSellerRatingAsync(int sellerId)
    {
        return await _reviewRepository.GetSellerRatingAsync(sellerId);
    }

    public async Task<bool> CanReviewAsync(int orderId, int buyerId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;
        if (order.BuyerId != buyerId) return false;
        if (order.Status != "delivered") return false;

        var existingReview = await _reviewRepository.ExistsByOrderIdAsync(orderId);

        return !existingReview;
    }

    public async Task<(decimal AverageRating, int TotalReviews)> GetProductRatingStatsAsync(int productId)
    {
        return await _reviewRepository.GetProductRatingStatsAsync(productId);
    }
}
