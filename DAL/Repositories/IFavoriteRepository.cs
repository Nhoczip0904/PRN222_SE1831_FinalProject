using DAL.Entities;

namespace DAL.Repositories;

public interface IFavoriteRepository
{
    Task<Favorite?> GetByIdAsync(int id);
    Task<Favorite?> GetByUserAndProductAsync(int userId, int productId);
    Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Product>> GetUserFavoriteProductsAsync(int userId);
    Task<List<int>> GetUserFavoriteProductIdsAsync(int userId);
    Task<int> GetCountByUserIdAsync(int userId);
    Task<bool> ExistsAsync(int userId, int productId);
    Task<Favorite> CreateAsync(Favorite favorite);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteByUserAndProductAsync(int userId, int productId);
}
