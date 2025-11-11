using BLL.Services;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Products;

public class CompareModel : PageModel
{
    private readonly IProductService _productService;
    private const string CompareSessionKey = "CompareProducts";

    public CompareModel(IProductService productService)
    {
        _productService = productService;
    }

    public List<CompareProductDto> Products { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var productIds = HttpContext.Session.GetObject<List<int>>(CompareSessionKey) ?? new List<int>();
        
        foreach (var id in productIds)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product != null)
            {
                Products.Add(new CompareProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    SellerName = product.SellerName,
                    Price = product.Price,
                    BatteryHealthPercent = product.BatteryHealthPercent,
                    Condition = product.Condition,
                    CategoryName = product.CategoryName,
                    FirstImage = product.Images?.Split(',').FirstOrDefault(),
                    Description = product.Description,
                    CreatedAt = product.CreatedAt
                });
            }
        }

        return Page();
    }

    public IActionResult OnGetRemove(int productId)
    {
        var productIds = HttpContext.Session.GetObject<List<int>>(CompareSessionKey) ?? new List<int>();
        productIds.Remove(productId);
        HttpContext.Session.SetObject(CompareSessionKey, productIds);
        
        return RedirectToPage();
    }

    public IActionResult OnPostAdd(int productId)
    {
        var productIds = HttpContext.Session.GetObject<List<int>>(CompareSessionKey) ?? new List<int>();
        
        if (!productIds.Contains(productId) && productIds.Count < 4)
        {
            productIds.Add(productId);
            HttpContext.Session.SetObject(CompareSessionKey, productIds);
        }
        
        return RedirectToPage();
    }
}
