using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Product
{
    public int Id { get; set; }

    public int? SellerId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }
    
    public int Quantity { get; set; }

    public int? BatteryHealthPercent { get; set; }

    public string Condition { get; set; } = null!;

    public string? Images { get; set; }

    public int? CategoryId { get; set; }

    public bool? IsActive { get; set; }
    
    public bool? IsSold { get; set; }

    public string? ApprovalStatus { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User? Seller { get; set; }

    public virtual User? Approver { get; set; }
}
