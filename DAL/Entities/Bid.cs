using System;

namespace DAL.Entities;

public partial class Bid
{
    public int Id { get; set; }

    public int AuctionId { get; set; }

    public int BidderId { get; set; }

    public decimal BidAmount { get; set; }

    public DateTime BidTime { get; set; }

    public bool IsWinning { get; set; }

    public virtual Auction Auction { get; set; } = null!;

    public virtual User Bidder { get; set; } = null!;
}
