using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Transactions;

public class IndexModel : PageModel
{
    private readonly IWalletService _walletService;
    private readonly IOrderService _orderService;

    public IndexModel(IWalletService walletService, IOrderService orderService)
    {
        _walletService = walletService;
        _orderService = orderService;
    }

    public List<TransactionViewModel> AllTransactions { get; set; } = new();
    public List<WalletTransaction> WalletTransactions { get; set; } = new();
    public List<TransactionViewModel> AuctionTransactions { get; set; } = new();
    public List<Order> Orders { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Get wallet transactions
        var walletResult = await _walletService.GetTransactionHistoryAsync(currentUser.Id);
        WalletTransactions = walletResult.Select(wt => new WalletTransaction
        {
            Id = wt.Id,
            Amount = wt.Amount,
            Description = wt.Description,
            CreatedAt = wt.CreatedAt,
            BalanceAfter = wt.BalanceAfter
        }).ToList();

        // Get orders - using dummy data for now
        Orders = new List<Order>();

        // Combine all transactions
        AllTransactions = new List<TransactionViewModel>();

        // Add wallet transactions
        foreach (var wt in WalletTransactions)
        {
            AllTransactions.Add(new TransactionViewModel
            {
                Type = "wallet",
                Description = wt.Description ?? "",
                Amount = wt.Amount,
                CreatedAt = wt.CreatedAt,
                Status = "completed",
                ReferenceId = null
            });
        }

        // Add order transactions
        foreach (var order in Orders)
        {
            AllTransactions.Add(new TransactionViewModel
            {
                Type = "order",
                Description = $"Đơn hàng #{order.Id}",
                Amount = -order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Status = order.Status ?? "pending",
                ReferenceId = order.Id
            });
        }

        // Filter auction transactions from wallet
        AuctionTransactions = AllTransactions
            .Where(t => t.Description != null && t.Description.Contains("đấu giá", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Sort by date descending
        AllTransactions = AllTransactions.OrderByDescending(t => t.CreatedAt).ToList();

        return Page();
    }
}

public class TransactionViewModel
{
    public string Type { get; set; } = string.Empty; // wallet, order, auction
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ReferenceId { get; set; }
}
