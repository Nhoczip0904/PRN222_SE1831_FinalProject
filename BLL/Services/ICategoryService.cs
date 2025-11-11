using BLL.DTOs;

namespace BLL.Services;

public interface ICategoryService
{
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<(bool Success, string Message)> CreateCategoryAsync(string name, string? description);
    Task<(bool Success, string Message)> UpdateCategoryAsync(int id, string name, string? description);
    Task<(bool Success, string Message)> DeleteCategoryAsync(int id);
}
