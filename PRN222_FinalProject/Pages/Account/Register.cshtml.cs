using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public RegisterDto RegisterDto { get; set; } = new RegisterDto();

    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.RegisterAsync(RegisterDto);

        if (result.Success)
        {
            IsSuccess = true;
            Message = result.Message;
            
            // Clear form
            ModelState.Clear();
            RegisterDto = new RegisterDto();
            
            // Redirect to login after 3 seconds
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToPage("/Account/Login");
        }
        else
        {
            IsSuccess = false;
            Message = result.Message;
            return Page();
        }
    }
}
