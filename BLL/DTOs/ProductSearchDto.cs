namespace BLL.DTOs;

public class ProductSearchDto
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Condition { get; set; }
    public int? MinBatteryHealth { get; set; }
    public string? SortBy { get; set; } // price_asc, price_desc, newest, oldest
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
