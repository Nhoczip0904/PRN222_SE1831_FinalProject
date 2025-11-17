using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PRN222_FinalProject.Pages.Admin.Users;

public class CreateModel : PageModel
{
    private readonly IAdminService _adminService;

    public CreateModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty]
    public AdminUserManagementDto UserDto { get; set; } = new AdminUserManagementDto();

    [BindProperty]
    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
    public string Password { get; set; } = null!;

    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public IActionResult OnGet()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || (currentUser.Role != "admin" && currentUser.Role != "staff"))
        {
            return RedirectToPage("/Account/Login");
        }

        // Set default values
        UserDto.Role = "member";
        UserDto.IsVerified = true;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || (currentUser.Role != "admin" && currentUser.Role != "staff"))
        {
            return RedirectToPage("/Account/Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _adminService.CreateUserAsync(UserDto, Password);

        if (result.Success)
        {
            TempData["Message"] = result.Message;
            TempData["IsSuccess"] = true;
            return RedirectToPage("/Admin/Users/Index");
        }
        else
        {
            IsSuccess = false;
            Message = result.Message;
            return Page();
        }
    }
}
