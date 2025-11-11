using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public IndexModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    public IEnumerable<ProductDto>? Products { get; set; }
    public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    public ProductSearchDto SearchDto { get; set; } = new ProductSearchDto();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public async Task OnGetAsync(string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy, int pageNumber = 1)
    {
        SearchDto = new ProductSearchDto
        {
            Keyword = keyword,
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            SortBy = sortBy ?? "newest",
            PageNumber = pageNumber,
            PageSize = 7
        };

        var result = await _productService.GetProductsWithPaginationAsync(SearchDto);
        Products = result.Products;
        TotalCount = result.TotalCount;
        TotalPages = result.TotalPages;

        Categories = await _categoryService.GetAllCategoriesAsync();
    }

    public IActionResult OnPostAddToCompare(int productId)
    {
        var productIds = HttpContext.Session.GetObject<List<int>>("CompareProducts") ?? new List<int>();
        
        if (!productIds.Contains(productId) && productIds.Count < 4)
        {
            productIds.Add(productId);
            HttpContext.Session.SetObject("CompareProducts", productIds);
            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào danh sách so sánh";
        }
        else if (productIds.Count >= 4)
        {
            TempData["ErrorMessage"] = "Chỉ có thể so sánh tối đa 4 sản phẩm";
        }
        
        return RedirectToPage();
    }
}

public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, System.Text.Json.JsonSerializer.Serialize(value));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : System.Text.Json.JsonSerializer.Deserialize<T>(value);
    }
}
