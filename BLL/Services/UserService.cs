using BLL.DTOs;
using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public UserService(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserDto);
    }

    public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetUsersWithPaginationAsync(int pageNumber, int pageSize)
    {
        var users = await _userRepository.GetAllWithPaginationAsync(pageNumber, pageSize);
        var totalCount = await _userRepository.GetTotalCountAsync();
        
        return (users.Select(MapToUserDto), totalCount);
    }

    public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return (false, "Người dùng không tồn tại");
        }

        // Check if phone is changed and already exists
        if (user.Phone != updateDto.Phone)
        {
            if (await _userRepository.PhoneExistsAsync(updateDto.Phone))
            {
                return (false, "Số điện thoại đã được sử dụng");
            }
        }

        // Update user info
        user.FullName = updateDto.FullName;
        user.Phone = updateDto.Phone;
        user.Address = updateDto.Address;
        user.UpdatedAt = DateTime.Now;

        await _userRepository.UpdateAsync(user);

        return (true, "Cập nhật thông tin thành công");
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return (false, "Người dùng không tồn tại");
        }

        // Validate current password
        if (!await _authService.ValidatePasswordAsync(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return (false, "Mật khẩu hiện tại không đúng");
        }

        // Update password
        user.PasswordHash = _authService.HashPassword(changePasswordDto.NewPassword);
        user.UpdatedAt = DateTime.Now;

        await _userRepository.UpdateAsync(user);

        return (true, "Đổi mật khẩu thành công");
    }

    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm)
    {
        var users = await _userRepository.SearchUsersAsync(searchTerm);
        return users.Select(MapToUserDto);
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
