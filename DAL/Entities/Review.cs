using System;

namespace DAL.Entities;

public class Review
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
    
    // Product rating
    public int ProductRating { get; set; } // 1-5
    public string? ProductReview { get; set; }
    
    // Seller rating
    public int SellerRating { get; set; } // 1-5
    public string? SellerReview { get; set; }
    
    // Images
    public string? Images { get; set; } // JSON array
    
    // Seller response
    public string? SellerResponse { get; set; }
    public DateTime? SellerResponseAt { get; set; }
    
    public bool IsVerified { get; set; } = true;
    public int IsHelpfulCount { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual User Buyer { get; set; } = null!;
    public virtual User Seller { get; set; } = null!;
}

public class SellerRating
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public virtual User Seller { get; set; } = null!;
}
