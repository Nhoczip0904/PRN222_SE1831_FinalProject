using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly EvBatteryTrading2Context _context;

    public ProductRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetAllActiveAsync()
    {
        return await _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Where(p => p.IsActive == true)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Where(p => p.IsActive == true && 
                        (p.ApprovalStatus == "approved" || p.ApprovalStatus == null) &&
                        (p.IsSold == false || p.IsSold == null))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetApprovedProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Where(p => p.ApprovalStatus == "approved")
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetPendingProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Where(p => p.ApprovalStatus == "pending")
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetBySellerIdAsync(int sellerId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.SellerId == sellerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.Products
            .Include(p => p.Seller)
            .Where(p => p.CategoryId == categoryId && p.IsActive == true)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice, string? condition, int? minBatteryHealth)
    {
        var query = _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Where(p => p.IsActive == true)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(keyword) || 
                                    p.Description.ToLower().Contains(keyword));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(condition))
        {
            query = query.Where(p => p.Condition == condition);
        }

        if (minBatteryHealth.HasValue)
        {
            query = query.Where(p => p.BatteryHealthPercent >= minBatteryHealth.Value);
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetWithPaginationAsync(int pageNumber, int pageSize, string? sortBy = null)
    {
        var query = _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Where(p => p.IsActive == true && (p.IsSold == false || p.IsSold == null))
            .AsQueryable();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "oldest" => query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt) // newest (default)
        };

        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.Now;
        product.UpdatedAt = DateTime.Now;
        product.IsActive = true;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.Now;

        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await GetByIdAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var product = await GetByIdAsync(id);
        if (product == null)
            return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Products.CountAsync();
    }
}
