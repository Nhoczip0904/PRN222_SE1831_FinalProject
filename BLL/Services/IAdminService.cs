using BLL.DTOs;

namespace BLL.Services;

public interface IAdminService
{
    Task<(IEnumerable<UserDto> Users, int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<(bool Success, string Message)> CreateUserAsync(AdminUserManagementDto userDto, string password);
    Task<(bool Success, string Message)> UpdateUserAsync(int userId, AdminUserManagementDto userDto);
    Task<(bool Success, string Message)> SuspendUserAsync(int userId);
    Task<(bool Success, string Message)> ActivateUserAsync(int userId);
    Task<(bool Success, string Message)> DeleteUserAsync(int userId);
    Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm);
}
