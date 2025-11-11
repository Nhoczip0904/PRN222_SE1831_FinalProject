using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BLL.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
    [StringLength(255, MinimumLength = 5, ErrorMessage = "Tên sản phẩm phải từ 5-255 ký tự")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Mô tả là bắt buộc")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Mô tả phải từ 10-1000 ký tự")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Giá là bắt buộc")]
    [Range(0.01, 999999999999, ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Số lượng là bắt buộc")]
    [Range(1, 10000, ErrorMessage = "Số lượng phải từ 1-10000")]
    public int Quantity { get; set; } = 1;

    [Range(0, 100, ErrorMessage = "Tình trạng pin phải từ 0-100%")]
    public int? BatteryHealthPercent { get; set; }

    [Required(ErrorMessage = "Tình trạng sản phẩm là bắt buộc")]
    [RegularExpression("^(poor|fair|good|new)$", 
        ErrorMessage = "Tình trạng không hợp lệ. Chọn từ dropdown")]
    public string Condition { get; set; } = null!;

    [Required(ErrorMessage = "Danh mục là bắt buộc")]
    public int CategoryId { get; set; }

    public List<IFormFile>? ImageFiles { get; set; }
}
