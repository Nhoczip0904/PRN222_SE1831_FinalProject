using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.Helpers;
using BLL.DTOs;
using System.ComponentModel.DataAnnotations;

namespace PRN222_FinalProject.Pages.Staff.Categories
{
    public class EditModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public EditModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();
        
        public int ProductCount { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục phải từ 2-100 ký tự")]
            public string Name { get; set; } = null!;

            [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
            public string? Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null || currentUser.Role != "staff")
            {
                return RedirectToPage("/Account/Login");
            }

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            ProductCount = category.ProductCount;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null || currentUser.Role != "staff")
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                // Reload product count if validation fails
                var category = await _categoryService.GetCategoryByIdAsync(Input.Id);
                if (category != null)
                {
                    ProductCount = category.ProductCount;
                }
                return Page();
            }

            var result = await _categoryService.UpdateCategoryAsync(Input.Id, Input.Name, Input.Description);
            
            Message = result.Message;
            Success = result.Success;

            if (result.Success)
            {
                return RedirectToPage("./Index");
            }

            // Reload product count if update fails
            var reloadCategory = await _categoryService.GetCategoryByIdAsync(Input.Id);
            if (reloadCategory != null)
            {
                ProductCount = reloadCategory.ProductCount;
            }

            return Page();
        }
    }
}
