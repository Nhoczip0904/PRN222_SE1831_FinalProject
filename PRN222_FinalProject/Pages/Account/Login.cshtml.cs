using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public LoginDto LoginDto { get; set; } = new LoginDto();

    public string? Message { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.LoginAsync(LoginDto);

        if (result.Success && result.User != null)
        {
            // Store user info in session
            HttpContext.Session.SetObjectAsJson("CurrentUser", result.User);
            
            // Set session timeout based on RememberMe
            if (LoginDto.RememberMe)
            {
                HttpContext.Session.SetInt32("SessionTimeout", 43200); // 30 days in minutes
            }
            else
            {
                HttpContext.Session.SetInt32("SessionTimeout", 1440); // 24 hours in minutes
            }

            // Redirect based on role
            if (result.User.Role == "admin")
            {
                return RedirectToPage("/Admin/Dashboard");
            }
            else if (result.User.Role == "staff")
            {
                return RedirectToPage("/Staff/Dashboard/Index");
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }
        else
        {
            Message = result.Message;
            return Page();
        }
    }
}
