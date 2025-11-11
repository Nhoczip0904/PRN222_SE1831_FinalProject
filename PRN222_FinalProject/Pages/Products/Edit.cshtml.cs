using BLL.Helpers;
using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Products;

public class EditModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public EditModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    [BindProperty]
    public UpdateProductDto UpdateDto { get; set; } = new UpdateProductDto();

    public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId != currentUser.Id)
        {
            TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa sản phẩm này";
            return RedirectToPage("/Products/MyProducts");
        }

        UpdateDto = new UpdateProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            BatteryHealthPercent = product.BatteryHealthPercent,
            Condition = product.Condition,
            CategoryId = product.CategoryId ?? 0,
            ExistingImages = product.Images,
            IsActive = product.IsActive ?? false
        };

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

        var result = await _productService.UpdateProductAsync(UpdateDto.Id, currentUser.Id, UpdateDto);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Products/MyProducts");
        }

        ModelState.AddModelError(string.Empty, result.Message);
        return Page();
    }
}
