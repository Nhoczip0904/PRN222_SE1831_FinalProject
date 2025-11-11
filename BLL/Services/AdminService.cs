using BLL.DTOs;
using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public AdminService(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        var users = await _userRepository.GetAllWithPaginationAsync(pageNumber, pageSize);
        var totalCount = await _userRepository.GetTotalCountAsync();
        
        return (users.Select(MapToUserDto), totalCount);
    }

    public async Task<(bool Success, string Message)> CreateUserAsync(AdminUserManagementDto userDto, string password)
    {
        // Validate unique email
        if (await _userRepository.EmailExistsAsync(userDto.Email))
        {
            return (false, "Email đã được sử dụng");
        }

        // Validate unique phone
        if (await _userRepository.PhoneExistsAsync(userDto.Phone))
        {
            return (false, "Số điện thoại đã được sử dụng");
        }

        var user = new User
        {
            Email = userDto.Email,
            Phone = userDto.Phone,
            FullName = userDto.FullName,
            Address = userDto.Address,
            PasswordHash = _authService.HashPassword(password),
            Role = userDto.Role,
            IsVerified = userDto.IsVerified ?? true, // Admin-created users are verified by default
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _userRepository.CreateAsync(user);

        return (true, "Tạo người dùng thành công");
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, AdminUserManagementDto userDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return (false, "Người dùng không tồn tại");
        }

        // Check if email is changed and already exists
        if (user.Email != userDto.Email)
        {
            if (await _userRepository.EmailExistsAsync(userDto.Email))
            {
                return (false, "Email đã được sử dụng");
            }
        }

        // Check if phone is changed and already exists
        if (user.Phone != userDto.Phone)
        {
            if (await _userRepository.PhoneExistsAsync(userDto.Phone))
            {
                return (false, "Số điện thoại đã được sử dụng");
            }
        }

        user.Email = userDto.Email;
        user.Phone = userDto.Phone;
        user.FullName = userDto.FullName;
        user.Address = userDto.Address;
        user.Role = userDto.Role;
        user.IsVerified = userDto.IsVerified;
        user.UpdatedAt = DateTime.Now;

        await _userRepository.UpdateAsync(user);

        return (true, "Cập nhật người dùng thành công");
    }

    public async Task<(bool Success, string Message)> SuspendUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return (false, "Người dùng không tồn tại");
        }

        if (user.Role == "admin")
        {
            return (false, "Không thể tạm ngưng tài khoản Admin");
        }

        user.IsVerified = false;
        user.UpdatedAt = DateTime.Now;

        await _userRepository.UpdateAsync(user);

        return (true, "Tạm ngưng tài khoản thành công");
    }

    public async Task<(bool Success, string Message)> ActivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return (false, "Người dùng không tồn tại");
        }

        user.IsVerified = true;
        user.UpdatedAt = DateTime.Now;

        await _userRepository.UpdateAsync(user);

        return (true, "Kích hoạt tài khoản thành công");
    }

    public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return (false, "Người dùng không tồn tại");
        }

        if (user.Role == "admin")
        {
            return (false, "Không thể xóa tài khoản Admin");
        }

        // Soft delete
        var result = await _userRepository.SoftDeleteAsync(userId);

        return result 
            ? (true, "Xóa người dùng thành công") 
            : (false, "Xóa người dùng thất bại");
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
