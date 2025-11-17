namespace BLL.DTOs;

public class ContractDto
{
    public int Id { get; set; }
    public int? BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }
    public string? BuyerAddress { get; set; }
    public int? SellerId { get; set; }
    public string? SellerName { get; set; }
    public string? SellerEmail { get; set; }
    public string? SellerPhone { get; set; }
    public string? SellerAddress { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string ShippingAddress { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Note { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
}
