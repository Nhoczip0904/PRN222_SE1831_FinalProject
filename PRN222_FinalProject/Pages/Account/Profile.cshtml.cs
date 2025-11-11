using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Account;

public class ProfileModel : PageModel
{
    private readonly IUserService _userService;

    public ProfileModel(IUserService userService)
    {
        _userService = userService;
    }

    public UserDto? CurrentUser { get; set; }

    [BindProperty]
    public UpdateProfileDto UpdateProfileDto { get; set; } = new UpdateProfileDto();

    [BindProperty]
    public ChangePasswordDto ChangePasswordDto { get; set; } = new ChangePasswordDto();

    public string? ProfileMessage { get; set; }
    public bool ProfileSuccess { get; set; }

    public string? PasswordMessage { get; set; }
    public bool PasswordSuccess { get; set; }

    public IActionResult OnGet()
    {
        // Check if user is logged in
        CurrentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Populate form with current user data
        UpdateProfileDto = new UpdateProfileDto
        {
            FullName = CurrentUser.FullName,
            Phone = CurrentUser.Phone,
            Address = CurrentUser.Address
        };

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        CurrentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _userService.UpdateProfileAsync(CurrentUser.Id, UpdateProfileDto);

        if (result.Success)
        {
            ProfileSuccess = true;
            ProfileMessage = result.Message;

            // Update session with new user data
            var updatedUser = await _userService.GetUserByIdAsync(CurrentUser.Id);
            if (updatedUser != null)
            {
                HttpContext.Session.SetObjectAsJson("CurrentUser", updatedUser);
                CurrentUser = updatedUser;
            }
        }
        else
        {
            ProfileSuccess = false;
            ProfileMessage = result.Message;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        CurrentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Only validate password fields
        ModelState.Remove("UpdateProfileDto.FullName");
        ModelState.Remove("UpdateProfileDto.Phone");
        ModelState.Remove("UpdateProfileDto.Address");

        if (!ModelState.IsValid)
        {
            // Reload profile data
            UpdateProfileDto = new UpdateProfileDto
            {
                FullName = CurrentUser.FullName,
                Phone = CurrentUser.Phone,
                Address = CurrentUser.Address
            };
            return Page();
        }

        var result = await _userService.ChangePasswordAsync(CurrentUser.Id, ChangePasswordDto);

        if (result.Success)
        {
            PasswordSuccess = true;
            PasswordMessage = result.Message;
            
            // Clear password form
            ModelState.Clear();
            ChangePasswordDto = new ChangePasswordDto();
        }
        else
        {
            PasswordSuccess = false;
            PasswordMessage = result.Message;
        }

        // Reload profile data
        UpdateProfileDto = new UpdateProfileDto
        {
            FullName = CurrentUser.FullName,
            Phone = CurrentUser.Phone,
            Address = CurrentUser.Address
        };

        return Page();
    }
}
