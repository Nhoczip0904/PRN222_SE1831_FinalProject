using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Transaction
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? Status { get; set; }

    public string? TransactionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
