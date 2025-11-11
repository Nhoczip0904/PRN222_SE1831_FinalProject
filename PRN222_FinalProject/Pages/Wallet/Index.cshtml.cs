using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Wallet;

public class IndexModel : PageModel
{
    private readonly IWalletService _walletService;

    public IndexModel(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public WalletDto? Wallet { get; set; }
    public IEnumerable<WalletTransactionDto>? Transactions { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Wallet = await _walletService.GetOrCreateWalletAsync(currentUser.Id);
        Transactions = await _walletService.GetTransactionHistoryAsync(currentUser.Id, 1, 50);

        return Page();
    }
}
