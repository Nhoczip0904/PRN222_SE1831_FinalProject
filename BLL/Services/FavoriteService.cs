using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services;

public interface IFavoriteService
{
    Task<(bool Success, string Message)> AddToFavoritesAsync(int userId, int productId);
    Task<(bool Success, string Message)> RemoveFromFavoritesAsync(int userId, int productId);
    Task<List<Product>> GetUserFavoritesAsync(int userId);
    Task<bool> IsFavoriteAsync(int userId, int productId);
    Task<int> GetFavoriteCountAsync(int userId);
    Task<List<int>> GetUserFavoriteIdsAsync(int userId);
}

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IProductRepository _productRepository;

    public FavoriteService(IFavoriteRepository favoriteRepository, IProductRepository productRepository)
    {
        _favoriteRepository = favoriteRepository;
        _productRepository = productRepository;
    }

    public async Task<(bool Success, string Message)> AddToFavoritesAsync(int userId, int productId)
    {
        // Check if product exists
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return (false, "Sản phẩm không tồn tại");
        }

        // Check if already in favorites
        var exists = await _favoriteRepository.ExistsAsync(userId, productId);
        if (exists)
        {
            return (false, "Sản phẩm đã có trong danh sách yêu thích");
        }

        // Add to favorites
        var favorite = new Favorite
        {
            UserId = userId,
            ProductId = productId
        };

        await _favoriteRepository.CreateAsync(favorite);

        return (true, "Đã thêm vào danh sách yêu thích");
    }

    public async Task<(bool Success, string Message)> RemoveFromFavoritesAsync(int userId, int productId)
    {
        var deleted = await _favoriteRepository.DeleteByUserAndProductAsync(userId, productId);
        
        if (!deleted)
        {
            return (false, "Sản phẩm không có trong danh sách yêu thích");
        }

        return (true, "Đã xóa khỏi danh sách yêu thích");
    }

    public async Task<List<Product>> GetUserFavoritesAsync(int userId)
    {
        var products = await _favoriteRepository.GetUserFavoriteProductsAsync(userId);
        return products.ToList();
    }

    public async Task<bool> IsFavoriteAsync(int userId, int productId)
    {
        return await _favoriteRepository.ExistsAsync(userId, productId);
    }

    public async Task<int> GetFavoriteCountAsync(int userId)
    {
        return await _favoriteRepository.GetCountByUserIdAsync(userId);
    }

    public async Task<List<int>> GetUserFavoriteIdsAsync(int userId)
    {
        return await _favoriteRepository.GetUserFavoriteProductIdsAsync(userId);
    }
}
