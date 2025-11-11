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

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = null!; // active, closed, cancelled

    public int? WinnerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User Seller { get; set; } = null!;

    public virtual User? Winner { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
}
