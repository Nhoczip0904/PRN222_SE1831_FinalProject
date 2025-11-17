using BLL.DTOs;

namespace BLL.Services;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
    Task<IEnumerable<ProductDto>> GetPendingProductsAsync();
    Task<(bool Success, string Message)> ApproveProductAsync(int productId, int adminId);
    Task<(bool Success, string Message)> RejectProductAsync(int productId, int adminId, string reason);
    Task<IEnumerable<ProductDto>> GetProductsBySellerIdAsync(int sellerId);
    Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto);
    Task<(IEnumerable<ProductDto> Products, int TotalCount, int TotalPages)> GetProductsWithPaginationAsync(ProductSearchDto searchDto);
    Task<(bool Success, string Message, int? ProductId)> CreateProductAsync(int sellerId, CreateProductDto createDto, String name);
    Task<(bool Success, string Message)> UpdateProductAsync(int productId, int sellerId, UpdateProductDto updateDto);
    Task<(bool Success, string Message)> DeleteProductAsync(int productId, int sellerId);
    Task<(bool Success, string Message)> DeactivateProductAsync(int productId, int sellerId);
    Task<string> SaveProductImagesAsync(List<Microsoft.AspNetCore.Http.IFormFile> images);
}
