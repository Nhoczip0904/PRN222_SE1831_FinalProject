using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-255 ký tự")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [RegularExpression(@"^(\+84|0)[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải là số Việt Nam hợp lệ")]
    public string Phone { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string? Address { get; set; }
}
