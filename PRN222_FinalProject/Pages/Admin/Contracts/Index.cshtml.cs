using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Admin.Contracts;

public class IndexModel : PageModel
{
    private readonly IContractService _contractService;

    public IndexModel(IContractService contractService)
    {
        _contractService = contractService;
    }

    public List<Contract> PendingContracts { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        PendingContracts = await _contractService.GetPendingContractsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int contractId)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _contractService.AdminApproveAsync(contractId, currentUser.Id, ipAddress);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int contractId, string reason)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập lý do từ chối!";
            return RedirectToPage();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _contractService.AdminRejectAsync(contractId, currentUser.Id, reason, ipAddress);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage();
    }
}
