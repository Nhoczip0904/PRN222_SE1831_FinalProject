using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Wallet
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal Balance { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
