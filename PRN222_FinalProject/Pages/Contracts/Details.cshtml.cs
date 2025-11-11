using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Contracts;

public class DetailsModel : PageModel
{
    private readonly IContractService _contractService;

    public DetailsModel(IContractService contractService)
    {
        _contractService = contractService;
    }

    public Contract? Contract { get; set; }
    public bool CanBuyerConfirm { get; set; }
    public bool CanSellerConfirm { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Contract = await _contractService.GetContractByIdAsync(id);
        if (Contract == null)
        {
            return NotFound();
        }

        if (Contract.BuyerId != currentUser.Id && Contract.SellerId != currentUser.Id && currentUser.Role != "admin")
        {
            return Forbid();
        }

        CanBuyerConfirm = Contract.BuyerId == currentUser.Id && !Contract.BuyerConfirmed && Contract.Status == "pending";
        CanSellerConfirm = Contract.SellerId == currentUser.Id && !Contract.SellerConfirmed && Contract.Status == "pending";

        return Page();
    }

    public async Task<IActionResult> OnPostBuyerConfirmAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _contractService.BuyerConfirmAsync(id, currentUser.Id, ipAddress);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSellerConfirmAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _contractService.SellerConfirmAsync(id, currentUser.Id, ipAddress);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage(new { id });
    }
}
