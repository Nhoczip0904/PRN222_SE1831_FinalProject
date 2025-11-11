using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly EvBatteryTrading2Context _context;

    public FavoriteRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<Favorite?> GetByIdAsync(int id)
    {
        return await _context.Favorites
            .Include(f => f.User)
            .Include(f => f.Product)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Favorite?> GetByUserAndProductAsync(int userId, int productId)
    {
        return await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
    }

    public async Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId)
    {
        return await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Product)
                .ThenInclude(p => p.Category)
            .Include(f => f.Product)
                .ThenInclude(p => p.Seller)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetUserFavoriteProductsAsync(int userId)
    {
        return await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Product)
                .ThenInclude(p => p.Category)
            .Include(f => f.Product)
                .ThenInclude(p => p.Seller)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.Product)
            .ToListAsync();
    }

    public async Task<List<int>> GetUserFavoriteProductIdsAsync(int userId)
    {
        return await _context.Favorites
            .Where(f => f.UserId == userId)
            .Select(f => f.ProductId)
            .ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(int userId)
    {
        return await _context.Favorites
            .CountAsync(f => f.UserId == userId);
    }

    public async Task<bool> ExistsAsync(int userId, int productId)
    {
        return await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.ProductId == productId);
    }

    public async Task<Favorite> CreateAsync(Favorite favorite)
    {
        favorite.CreatedAt = DateTime.Now;
        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();
        return favorite;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var favorite = await _context.Favorites.FindAsync(id);
        if (favorite == null)
            return false;

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByUserAndProductAsync(int userId, int productId)
    {
        var favorite = await GetByUserAndProductAsync(userId, productId);
        if (favorite == null)
            return false;

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return true;
    }
}
