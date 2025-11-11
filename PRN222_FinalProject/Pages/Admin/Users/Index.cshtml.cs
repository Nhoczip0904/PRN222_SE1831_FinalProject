using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly IAdminService _adminService;
    private const int PageSize = 10;

    public IndexModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public IEnumerable<UserDto>? Users { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync(int page = 1, string? searchTerm = null)
    {
        // Check if user is admin
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        CurrentPage = page;
        SearchTerm = searchTerm;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchResults = await _adminService.SearchUsersAsync(searchTerm);
            Users = searchResults.Skip((page - 1) * PageSize).Take(PageSize);
            TotalPages = (int)Math.Ceiling(searchResults.Count() / (double)PageSize);
        }
        else
        {
            var result = await _adminService.GetAllUsersAsync(page, PageSize);
            Users = result.Users;
            TotalPages = (int)Math.Ceiling(result.TotalCount / (double)PageSize);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSuspendAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _adminService.SuspendUserAsync(id);
        
        TempData["Message"] = result.Message;
        TempData["IsSuccess"] = result.Success;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _adminService.ActivateUserAsync(id);
        
        TempData["Message"] = result.Message;
        TempData["IsSuccess"] = result.Success;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _adminService.DeleteUserAsync(id);
        
        TempData["Message"] = result.Message;
        TempData["IsSuccess"] = result.Success;

        return RedirectToPage();
    }
}
