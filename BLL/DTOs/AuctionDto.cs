namespace BLL.DTOs;

public class AuctionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductImage { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = null!;
    public decimal StartingPrice { get; set; }
    public decimal? CurrentPrice { get; set; }
    public decimal? ReservePrice { get; set; }

    public decimal BidIncrement { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = null!;
    public string ApprovalStatus { get; set; } = "pending";
    public int? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public string? ApprovalReason { get; set; }
    public int? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public int TotalBids { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool IsExpired => DateTime.Now > EndTime;
    public TimeSpan TimeRemaining => EndTime > DateTime.Now ? EndTime - DateTime.Now : TimeSpan.Zero;
}
