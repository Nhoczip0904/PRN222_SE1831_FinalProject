namespace BLL.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int? BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public int? SellerId { get; set; }
    public string? SellerName { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string ShippingAddress { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
}
