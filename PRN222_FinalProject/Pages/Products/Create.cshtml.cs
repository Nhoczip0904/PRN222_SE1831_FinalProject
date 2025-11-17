using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Products;

public class CreateModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public CreateModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    [BindProperty]
    public CreateProductDto CreateDto { get; set; } = new CreateProductDto();

    public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Categories = await _categoryService.GetAllCategoriesAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        Categories = await _categoryService.GetAllCategoriesAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _productService.CreateProductAsync(currentUser.Id, CreateDto, currentUser.FullName);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Products/MyProducts");
        }

        ModelState.AddModelError(string.Empty, result.Message);
        return Page();
    }
}
