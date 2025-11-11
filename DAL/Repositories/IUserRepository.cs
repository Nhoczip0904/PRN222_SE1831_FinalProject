using DAL.Entities;

namespace DAL.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneAsync(string phone);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetAllWithPaginationAsync(int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phone);
    Task<bool> SoftDeleteAsync(int id);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
}
