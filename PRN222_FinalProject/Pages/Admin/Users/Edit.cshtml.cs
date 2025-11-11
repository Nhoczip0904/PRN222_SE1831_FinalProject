using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Admin.Users;

public class EditModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly IUserService _userService;

    public EditModel(IAdminService adminService, IUserService userService)
    {
        _adminService = adminService;
        _userService = userService;
    }

    [BindProperty]
    public AdminUserManagementDto UserDto { get; set; } = new AdminUserManagementDto();

    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        UserDto = new AdminUserManagementDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            FullName = user.FullName,
            Address = user.Address,
            Role = user.Role,
            IsVerified = user.IsVerified
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _adminService.UpdateUserAsync(UserDto.Id, UserDto);

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
