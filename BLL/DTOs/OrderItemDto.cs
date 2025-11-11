namespace BLL.DTOs;

public class OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    public decimal TotalPrice => UnitPrice * Quantity;
}
