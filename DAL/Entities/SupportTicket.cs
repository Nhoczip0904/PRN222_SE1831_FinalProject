using System;
using System.Collections.Generic;

namespace DAL.Entities;

public class SupportTicket
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = null!;
    public int UserId { get; set; }
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
    
    public string Category { get; set; } = null!; // product_issue, delivery_issue, payment_issue, other
    public string Subject { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Images { get; set; } // JSON array
    
    public string Status { get; set; } = "open"; // open, in_progress, resolved, closed
    public string Priority { get; set; } = "normal"; // low, normal, high, urgent
    
    public int? AssignedTo { get; set; } // Admin ID
    public string? AdminNotes { get; set; }
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
    public virtual User? AssignedAdmin { get; set; }
    public virtual ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
}

public class TicketMessage
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = null!;
    public string? Attachments { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    // Navigation properties
    public virtual SupportTicket Ticket { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
