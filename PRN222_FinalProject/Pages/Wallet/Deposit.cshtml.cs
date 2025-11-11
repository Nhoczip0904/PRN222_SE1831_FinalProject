using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Wallet;

public class DepositModel : PageModel
{
    private readonly IWalletService _walletService;
    private readonly IVNPayService _vnPayService;

    public DepositModel(IWalletService walletService, IVNPayService vnPayService)
    {
        _walletService = walletService;
        _vnPayService = vnPayService;
    }

    [BindProperty]
    public DepositDto DepositDto { get; set; } = new DepositDto();

    public decimal CurrentBalance { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var wallet = await _walletService.GetOrCreateWalletAsync(currentUser.Id);
        CurrentBalance = wallet.Balance;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var wallet = await _walletService.GetOrCreateWalletAsync(currentUser.Id);
        CurrentBalance = wallet.Balance;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _walletService.DepositAsync(currentUser.Id, DepositDto);

        if (result.Success)
        {
            TempData["SuccessMessage"] = $"{result.Message}. Số dư mới: {result.NewBalance:N0} VND";
            return RedirectToPage("/Wallet/Index");
        }

        ModelState.AddModelError(string.Empty, result.Message);
        return Page();
    }

    public async Task<IActionResult> OnPostVNPayAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var wallet = await _walletService.GetOrCreateWalletAsync(currentUser.Id);
        CurrentBalance = wallet.Balance;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (DepositDto.Amount < 10000)
        {
            ModelState.AddModelError("DepositDto.Amount", "Số tiền nạp tối thiểu là 10,000 VND");
            return Page();
        }

        // Create VNPay payment URL
        string transactionId = $"WALLET_{currentUser.Id}_{DateTime.Now:yyyyMMddHHmmss}";
        string orderInfo = $"Nap tien vi - {currentUser.FullName}";
        string returnUrl = $"{Request.Scheme}://{Request.Host}/Payment/VNPayCallback";

        string paymentUrl = _vnPayService.CreatePaymentUrl(
            transactionId,
            DepositDto.Amount,
            orderInfo,
            returnUrl
        );

        return Redirect(paymentUrl);
    }

    public async Task<IActionResult> OnPostDemoAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var wallet = await _walletService.GetOrCreateWalletAsync(currentUser.Id);
        CurrentBalance = wallet.Balance;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (DepositDto.Amount < 10000)
        {
            ModelState.AddModelError("DepositDto.Amount", "Số tiền nạp tối thiểu là 10,000 VND");
            return Page();
        }

        var result = await _walletService.AddBalanceAsync(
            currentUser.Id,
            DepositDto.Amount,
            DepositDto.Description ?? "Nạp tiền vào ví (Demo)",
            null,
            "deposit_demo"
        );

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Nạp tiền thành công!";
            return RedirectToPage("/Wallet/Index");
        }

        ModelState.AddModelError("", result.Message);
        return Page();
    }
}
