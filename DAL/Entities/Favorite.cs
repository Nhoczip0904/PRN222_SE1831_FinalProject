using System;

namespace DAL.Entities;

public class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
