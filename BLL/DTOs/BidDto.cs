namespace BLL.DTOs;

public class BidDto
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public int BidderId { get; set; }
    public string BidderName { get; set; } = null!;
    public decimal BidAmount { get; set; }
    public DateTime BidTime { get; set; }
    public bool IsWinning { get; set; }
}
