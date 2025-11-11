using System;
using System.Collections.Generic;

namespace DAL.Entities;

public class Contract
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
    public string ContractNumber { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    
    // Xác nhận của buyer
    public bool BuyerConfirmed { get; set; }
    public DateTime? BuyerConfirmedAt { get; set; }
    
    // Xác nhận của seller
    public bool SellerConfirmed { get; set; }
    public DateTime? SellerConfirmedAt { get; set; }
    
    // Duyệt của admin
    public bool AdminApproved { get; set; }
    public DateTime? AdminApprovedAt { get; set; }
    public int? AdminApprovedBy { get; set; }
    
    // Trạng thái
    public string Status { get; set; } = "pending"; // pending, confirmed, approved, rejected, cancelled
    public string? RejectionReason { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual User Buyer { get; set; } = null!;
    public virtual User Seller { get; set; } = null!;
    public virtual User? AdminApprover { get; set; }
    public virtual ICollection<ContractConfirmation> Confirmations { get; set; } = new List<ContractConfirmation>();
}

public class ContractConfirmation
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int UserId { get; set; }
    public string UserRole { get; set; } = null!; // buyer, seller, admin
    public string Action { get; set; } = null!; // confirmed, rejected, approved
    public string? Note { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Contract Contract { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
