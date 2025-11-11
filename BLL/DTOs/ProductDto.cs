namespace BLL.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public int? SellerId { get; set; }
    public string SellerName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int? BatteryHealthPercent { get; set; }
    public string Condition { get; set; } = null!;
    public string? Images { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
