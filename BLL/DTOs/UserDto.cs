namespace BLL.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Address { get; set; }
    public string Role { get; set; } = null!;
    public bool? IsVerified { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
