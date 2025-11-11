using System;

namespace DAL.Entities;

public class SystemRevenue
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal CommissionRate { get; set; } // 0.25 for 25%
    public decimal CommissionAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public virtual Order Order { get; set; } = null!;
}
