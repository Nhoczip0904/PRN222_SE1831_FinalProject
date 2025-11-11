using System.ComponentModel.DataAnnotations;
using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Admin.Categories;

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
        if (currentUser == null || currentUser.Role != "admin")
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
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        if (!ModelState.IsValid)
        {
            var category = await _categoryService.GetCategoryByIdAsync(Input.Id);
            if (category != null)
            {
                ProductCount = category.ProductCount;
            }
            return Page();
        }

        var result = await _categoryService.UpdateCategoryAsync(Input.Id, Input.Name, Input.Description);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Message);
        
        var cat = await _categoryService.GetCategoryByIdAsync(Input.Id);
        if (cat != null)
        {
            ProductCount = cat.ProductCount;
        }
        
        return Page();
    }
}
