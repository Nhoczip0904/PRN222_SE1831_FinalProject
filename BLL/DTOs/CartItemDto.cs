namespace BLL.DTOs;

public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Image { get; set; }
    public int? SellerId { get; set; }
    public string? SellerName { get; set; }
    public bool IsAvailable { get; set; }
    
    public decimal TotalPrice => Price * Quantity;
}
