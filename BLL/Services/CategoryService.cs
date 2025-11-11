using BLL.DTOs;
using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdWithProductsAsync(id);
        return category != null ? MapToCategoryDto(category) : null;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapToCategoryDto);
    }

    public async Task<(bool Success, string Message)> CreateCategoryAsync(string name, string? description)
    {
        try
        {
            // Check if category name already exists
            var existingCategories = await _categoryRepository.GetAllAsync();
            if (existingCategories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Tên danh mục đã tồn tại");
            }

            var category = new Category
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _categoryRepository.CreateAsync(category);
            return (true, "Tạo danh mục thành công");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateCategoryAsync(int id, string name, string? description)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return (false, "Danh mục không tồn tại");
            }

            // Check if new name already exists (except current category)
            var existingCategories = await _categoryRepository.GetAllAsync();
            if (existingCategories.Any(c => c.Id != id && c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Tên danh mục đã tồn tại");
            }

            category.Name = name;
            category.Description = description;
            category.UpdatedAt = DateTime.Now;

            await _categoryRepository.UpdateAsync(category);
            return (true, "Cập nhật danh mục thành công");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdWithProductsAsync(id);
            if (category == null)
            {
                return (false, "Danh mục không tồn tại");
            }

            if (category.Products != null && category.Products.Any())
            {
                return (false, "Không thể xóa danh mục có sản phẩm");
            }

            await _categoryRepository.DeleteAsync(id);
            return (true, "Xóa danh mục thành công");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    private CategoryDto MapToCategoryDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ProductCount = category.Products?.Count ?? 0
        };
    }
}
