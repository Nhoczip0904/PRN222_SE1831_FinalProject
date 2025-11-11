using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly EvBatteryTrading2Context _context;

    public UserRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByPhoneAsync(string phone)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Phone == phone);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        user.CreatedAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.Now;
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await GetByIdAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> PhoneExistsAsync(string phone)
    {
        return await _context.Users.AnyAsync(u => u.Phone == phone);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var user = await GetByIdAsync(id);
        if (user == null)
            return false;

        // Soft delete: set IsVerified to false to suspend account
        user.IsVerified = false;
        user.UpdatedAt = DateTime.Now;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        searchTerm = searchTerm.ToLower();
        
        return await _context.Users
            .Where(u => u.Email.ToLower().Contains(searchTerm) ||
                       u.FullName.ToLower().Contains(searchTerm) ||
                       u.Phone.Contains(searchTerm))
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }
}
