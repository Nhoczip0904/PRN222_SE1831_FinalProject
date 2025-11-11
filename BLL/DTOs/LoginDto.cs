using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}
