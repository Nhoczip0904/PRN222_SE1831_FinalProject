using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Contracts;

public class IndexModel : PageModel
{
    private readonly IContractService _contractService;

    public IndexModel(IContractService contractService)
    {
        _contractService = contractService;
    }

    public List<Contract> Contracts { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Contracts = await _contractService.GetUserContractsAsync(currentUser.Id);
        return Page();
    }
}
