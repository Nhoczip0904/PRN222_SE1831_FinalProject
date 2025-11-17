namespace BLL.DTOs;

public class ContractListDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = null!;
    public int SellerId { get; set; }
    public string SellerName { get; set; } = null!;
    public decimal OrderAmount { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool BuyerConfirmed { get; set; }
    public bool SellerConfirmed { get; set; }
    public bool AdminApproved { get; set; }
}
