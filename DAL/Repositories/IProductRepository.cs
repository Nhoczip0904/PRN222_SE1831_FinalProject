using DAL.Entities;

namespace DAL.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<IEnumerable<Product>> GetApprovedProductsAsync();
    Task<IEnumerable<Product>> GetPendingProductsAsync();
    Task<IEnumerable<Product>> GetBySellerIdAsync(int sellerId);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Product>> SearchAsync(string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice, string? condition, int? minBatteryHealth);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetWithPaginationAsync(int pageNumber, int pageSize, string? sortBy = null);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeactivateAsync(int id);
    Task<int> GetTotalCountAsync();
}
