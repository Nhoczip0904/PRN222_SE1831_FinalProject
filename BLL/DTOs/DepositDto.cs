using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class DepositDto
{
    [Required(ErrorMessage = "Số tiền là bắt buộc")]
    [Range(10000, 1000000000, ErrorMessage = "Số tiền phải từ 10,000 đến 1,000,000,000 VND")]
    public decimal Amount { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    public string? Description { get; set; }
}
