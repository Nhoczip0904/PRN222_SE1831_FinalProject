namespace BLL.DTOs;

public class ContractDetailsDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = null!;
    public string BuyerEmail { get; set; } = null!;
    public string BuyerPhone { get; set; } = null!;
    public string BuyerAddress { get; set; } = null!;
    public int SellerId { get; set; }
    public string SellerName { get; set; } = null!;
    public string SellerEmail { get; set; } = null!;
    public string SellerPhone { get; set; } = null!;
    public string SellerAddress { get; set; } = null!;
    public decimal OrderAmount { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool BuyerConfirmed { get; set; }
    public bool SellerConfirmed { get; set; }
    public bool AdminApproved { get; set; }
    public DateTime? BuyerConfirmedAt { get; set; }
    public DateTime? SellerConfirmedAt { get; set; }
    public DateTime? AdminApprovedAt { get; set; }
    public string? AdminApprovedBy { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? BuyerIpAddress { get; set; }
    public string? SellerIpAddress { get; set; }
    public string? AdminIpAddress { get; set; }
}
