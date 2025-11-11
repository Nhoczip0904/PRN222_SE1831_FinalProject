using System;

namespace DAL.Entities;

public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public int? ProductId { get; set; }
    public string? Subject { get; set; }
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    // Navigation properties
    public virtual User Sender { get; set; } = null!;
    public virtual User Receiver { get; set; } = null!;
    public virtual Product? Product { get; set; }
}
