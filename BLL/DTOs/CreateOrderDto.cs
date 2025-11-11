using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class CreateOrderDto
{
    [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string ShippingAddress { get; set; } = null!;

    public string? PaymentMethod { get; set; }
    public string? Note { get; set; }
}
