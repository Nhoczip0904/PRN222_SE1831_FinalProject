using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.DTOs;
using BLL.Services;
using BLL.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_FinalProject.Pages.Staff.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public IndexModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public List<CategoryDto> Categories { get; set; } = new();
        public string? Message { get; set; }
        public bool Success { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null || currentUser.Role != "staff")
            {
                return RedirectToPage("/Account/Login");
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            Categories = categories.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null || currentUser.Role != "staff")
            {
                return RedirectToPage("/Account/Login");
            }

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                Message = "Danh mục không tồn tại";
                Success = false;
            }
            else if (category.ProductCount > 0)
            {
                Message = "Không thể xóa danh mục có sản phẩm";
                Success = false;
            }
            else
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                Message = result.Message;
                Success = result.Success;
            }

            // Reload categories after operation
            var categories = await _categoryService.GetAllCategoriesAsync();
            Categories = categories.ToList();

            return Page();
        }
    }
}
