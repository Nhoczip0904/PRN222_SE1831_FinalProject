namespace BLL.DTOs;

public class CompareProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? SellerName { get; set; }
    public decimal Price { get; set; }
    public int? BatteryHealthPercent { get; set; }
    public string Condition { get; set; } = null!;
    public string? CategoryName { get; set; }
    public string? FirstImage { get; set; }
    public string Description { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
}
