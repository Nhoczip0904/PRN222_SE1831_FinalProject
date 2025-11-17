using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BLL.Services;
using BLL.Helpers;
using BLL.DTOs;
using System.ComponentModel.DataAnnotations;

namespace PRN222_FinalProject.Pages.Staff.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public CreateModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục phải từ 2-100 ký tự")]
            public string Name { get; set; } = null!;

            [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
            public string? Description { get; set; }
        }

        public string? Message { get; set; }
        public bool Success { get; set; }

        public IActionResult OnGet()
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
            if (currentUser == null || currentUser.Role != "staff")
            {
                return RedirectToPage("/Account/Login");
            }

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
                return Page();
            }

            var result = await _categoryService.CreateCategoryAsync(Input.Name, Input.Description);
            
            Message = result.Message;
            Success = result.Success;

            if (result.Success)
            {
                return RedirectToPage("./Index");
            }

            return Page();
        }
    }
}
