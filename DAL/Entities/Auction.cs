using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Auction
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int SellerId { get; set; }

    public decimal StartingPrice { get; set; }

    public decimal? CurrentPrice { get; set; }

    public decimal? ReservePrice { get; set; }

    public decimal BidIncrement { get; set; } = 10000; // Default 10,000 VND

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = null!; // active, closed, cancelled
    
    public string ApprovalStatus { get; set; } = "pending"; // pending, approved, cancelled
    
    public int? ApprovedById { get; set; }
    
    public string? ApprovalReason { get; set; } // lý do từ chối

    public int? WinnerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User Seller { get; set; } = null!;
    
    public virtual User? ApprovedBy { get; set; }

    public virtual User? Winner { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
}
