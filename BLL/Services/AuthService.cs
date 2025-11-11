using BLL.DTOs;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;
    private const int MaxLoginAttempts = 5;
    private const int LockoutMinutes = 60;

    public AuthService(IUserRepository userRepository, IMemoryCache cache)
    {
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<(bool Success, string Message, UserDto? User)> RegisterAsync(RegisterDto registerDto)
    {
        // Validate unique email
        if (await _userRepository.EmailExistsAsync(registerDto.Email))
        {
            return (false, "Email đã được sử dụng", null);
        }

        // Validate unique phone
        if (await _userRepository.PhoneExistsAsync(registerDto.Phone))
        {
            return (false, "Số điện thoại đã được sử dụng", null);
        }

        // Create new user
        var user = new User
        {
            Email = registerDto.Email,
            Phone = registerDto.Phone,
            FullName = registerDto.FullName,
            Address = registerDto.Address,
            PasswordHash = HashPassword(registerDto.Password),
            Role = "member",
            IsVerified = false, // Require email verification
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var createdUser = await _userRepository.CreateAsync(user);

        var userDto = MapToUserDto(createdUser);

        return (true, "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.", userDto);
    }

    public async Task<(bool Success, string Message, UserDto? User)> LoginAsync(LoginDto loginDto)
    {
        // Check if account is locked
        if (!await CheckLoginAttemptsAsync(loginDto.Email))
        {
            return (false, $"Tài khoản đã bị khóa do đăng nhập sai quá {MaxLoginAttempts} lần. Vui lòng thử lại sau {LockoutMinutes} phút.", null);
        }

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        
        if (user == null)
        {
            await RecordLoginAttemptAsync(loginDto.Email, false);
            return (false, "Email hoặc mật khẩu không đúng", null);
        }

        // Validate password
        if (!await ValidatePasswordAsync(loginDto.Password, user.PasswordHash))
        {
            await RecordLoginAttemptAsync(loginDto.Email, false);
            return (false, "Email hoặc mật khẩu không đúng", null);
        }

        // Check if account is verified
        if (user.IsVerified == false)
        {
            return (false, "Tài khoản chưa được xác thực. Vui lòng kiểm tra email.", null);
        }

        // Successful login
        await RecordLoginAttemptAsync(loginDto.Email, true);

        var userDto = MapToUserDto(user);

        return (true, "Đăng nhập thành công", userDto);
    }

    public async Task<bool> ValidatePasswordAsync(string password, string passwordHash)
    {
        return await Task.FromResult(BCrypt.Net.BCrypt.Verify(password, passwordHash));
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public async Task<bool> CheckLoginAttemptsAsync(string email)
    {
        var cacheKey = $"login_attempts_{email}";
        
        if (_cache.TryGetValue(cacheKey, out int attempts))
        {
            return attempts < MaxLoginAttempts;
        }

        return await Task.FromResult(true);
    }

    public async Task RecordLoginAttemptAsync(string email, bool success)
    {
        var cacheKey = $"login_attempts_{email}";

        if (success)
        {
            // Clear attempts on successful login
            _cache.Remove(cacheKey);
        }
        else
        {
            // Increment failed attempts
            if (_cache.TryGetValue(cacheKey, out int attempts))
            {
                attempts++;
            }
            else
            {
                attempts = 1;
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(LockoutMinutes));

            _cache.Set(cacheKey, attempts, cacheOptions);
        }

        await Task.CompletedTask;
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            FullName = user.FullName,
            Address = user.Address,
            Role = user.Role,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
