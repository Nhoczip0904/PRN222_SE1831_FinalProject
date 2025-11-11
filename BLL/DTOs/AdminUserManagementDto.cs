using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class AdminUserManagementDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(255, MinimumLength = 2)]
    public string FullName { get; set; } = null!;

    public string? Address { get; set; }

    [Required(ErrorMessage = "Vai trò là bắt buộc")]
    public string Role { get; set; } = null!;

    public bool? IsVerified { get; set; }

    public string? Status { get; set; } // active, suspended
}
