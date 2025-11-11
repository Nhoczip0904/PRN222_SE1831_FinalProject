using BLL.DTOs;

namespace BLL.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, UserDto? User)> RegisterAsync(RegisterDto registerDto);
    Task<(bool Success, string Message, UserDto? User)> LoginAsync(LoginDto loginDto);
    Task<bool> ValidatePasswordAsync(string password, string passwordHash);
    string HashPassword(string password);
    Task<bool> CheckLoginAttemptsAsync(string email);
    Task RecordLoginAttemptAsync(string email, bool success);
}
