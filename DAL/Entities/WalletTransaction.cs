using System;

namespace DAL.Entities;

public partial class WalletTransaction
{
    public int Id { get; set; }

    public int WalletId { get; set; }

    public string TransactionType { get; set; } = null!; // deposit, withdraw, bid_hold, bid_release, payment, refund

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? Description { get; set; }

    public int? ReferenceId { get; set; }

    public string? ReferenceType { get; set; } // auction, order

    public DateTime CreatedAt { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
