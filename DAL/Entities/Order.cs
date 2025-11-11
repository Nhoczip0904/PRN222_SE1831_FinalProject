using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Order
{
    public int Id { get; set; }

    public int? BuyerId { get; set; }

    public DateTime? CreatedAt { get; set; }
    
    public DateTime? OrderDate { get; set; }

    public int? SellerId { get; set; }

    public string ShippingAddress { get; set; } = null!;

    public string? Status { get; set; }

    public decimal TotalAmount { get; set; }
    
    public string? PaymentMethod { get; set; }
    
    public string? Note { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? Buyer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User? Seller { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
