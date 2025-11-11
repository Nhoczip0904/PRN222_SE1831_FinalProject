using BLL.DTOs;

namespace BLL.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<(IEnumerable<UserDto> Users, int TotalCount)> GetUsersWithPaginationAsync(int pageNumber, int pageSize);
    Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileDto updateDto);
    Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
    Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm);
}
