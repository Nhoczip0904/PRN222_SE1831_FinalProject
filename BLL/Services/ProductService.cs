using BLL.DTOs;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;

namespace BLL.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly INotificationService _notificationService;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, INotificationService notificationService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _notificationService = notificationService;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id);
        return product != null ? MapToProductDto(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToProductDto);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        var products = await _productRepository.GetActiveProductsAsync();
        return products.Select(MapToProductDto);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsBySellerIdAsync(int sellerId)
    {
        var products = await _productRepository.GetBySellerIdAsync(sellerId);
        return products.Select(MapToProductDto);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
    {
        var products = await _productRepository.GetByCategoryIdAsync(categoryId);
        return products.Select(MapToProductDto);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto)
    {
        var products = await _productRepository.SearchAsync(
            searchDto.Keyword,
            searchDto.CategoryId,
            searchDto.MinPrice,
            searchDto.MaxPrice,
            searchDto.Condition,
            searchDto.MinBatteryHealth
        );

        return products.Select(MapToProductDto);
    }

    public async Task<(IEnumerable<ProductDto> Products, int TotalCount, int TotalPages)> GetProductsWithPaginationAsync(ProductSearchDto searchDto)
    {
        IEnumerable<Product> products;
        int totalCount;

        if (!string.IsNullOrWhiteSpace(searchDto.Keyword) || searchDto.CategoryId.HasValue || 
            searchDto.MinPrice.HasValue || searchDto.MaxPrice.HasValue || 
            !string.IsNullOrWhiteSpace(searchDto.Condition) || searchDto.MinBatteryHealth.HasValue)
        {
            // Search with filters
            var allProducts = await _productRepository.SearchAsync(
                searchDto.Keyword,
                searchDto.CategoryId,
                searchDto.MinPrice,
                searchDto.MaxPrice,
                searchDto.Condition,
                searchDto.MinBatteryHealth
            );

            totalCount = allProducts.Count();
            products = allProducts
                .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize);
        }
        else
        {
            // Get all with pagination
            var result = await _productRepository.GetWithPaginationAsync(
                searchDto.PageNumber,
                searchDto.PageSize,
                searchDto.SortBy
            );
            products = result.Products;
            totalCount = result.TotalCount;
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);
        var productDtos = products.Select(MapToProductDto);

        return (productDtos, totalCount, totalPages);
    }

    public async Task<(bool Success, string Message, int? ProductId)> CreateProductAsync(int sellerId, CreateProductDto createDto, string sellerName)
    {
        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId);
        if (category == null)
        {
            return (false, "Danh mục không tồn tại", null);
        }

        // Validate constraints
        if (createDto.Price <= 0)
        {
            return (false, "Giá phải lớn hơn 0", null);
        }

        if (createDto.BatteryHealthPercent.HasValue && 
            (createDto.BatteryHealthPercent < 0 || createDto.BatteryHealthPercent > 100))
        {
            return (false, "Tình trạng pin phải từ 0-100%", null);
        }

        // Validate condition (accept both English and Vietnamese)
        var validConditions = new[] { "poor", "fair", "good", "new" };
        if (!validConditions.Contains(createDto.Condition.ToLower()))
        {
            return (false, "Tình trạng sản phẩm không hợp lệ. Chọn: new, good, fair, hoặc poor", null);
        }

        // Save images
        string? imageUrls = null;
        if (createDto.ImageFiles != null && createDto.ImageFiles.Any())
        {
            imageUrls = await SaveProductImagesAsync(createDto.ImageFiles);
        }

        var product = new Product
        {
            SellerId = sellerId,
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            Quantity = createDto.Quantity,
            BatteryHealthPercent = createDto.BatteryHealthPercent,
            Condition = createDto.Condition,
            CategoryId = createDto.CategoryId,
            Images = imageUrls,
            IsActive = false,
            IsSold = true,        // Ẩn sản phẩm cho đến khi được duyệt
            ApprovalStatus = "pending", // Mặc định chờ duyệt
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var createdProduct = await _productRepository.CreateAsync(product);

        // Notify admin about new product pending approval
        await _notificationService.NotifyAdminNewProductAsync(
         createdProduct.Id,
         createdProduct.Name,
         sellerName ?? "Unknown" // Dùng tên được truyền vào
     );

        return (true, "Đăng sản phẩm thành công. Đang chờ admin duyệt.", createdProduct.Id);
    }

    public async Task<(bool Success, string Message)> UpdateProductAsync(int productId, int sellerId, UpdateProductDto updateDto)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        
        if (product == null)
        {
            return (false, "Sản phẩm không tồn tại");
        }

        // Check ownership
        if (product.SellerId != sellerId)
        {
            return (false, "Bạn không có quyền chỉnh sửa sản phẩm này");
        }

        // Validate category
        var category = await _categoryRepository.GetByIdAsync(updateDto.CategoryId);
        if (category == null)
        {
            return (false, "Danh mục không tồn tại");
        }

        // Validate constraints
        if (updateDto.Price <= 0)
        {
            return (false, "Giá phải lớn hơn 0");
        }

        if (updateDto.BatteryHealthPercent.HasValue && 
            (updateDto.BatteryHealthPercent < 0 || updateDto.BatteryHealthPercent > 100))
        {
            return (false, "Tình trạng pin phải từ 0-100%");
        }

        // Validate condition (accept both English and Vietnamese)
        var validConditions = new[] { "poor", "fair", "good", "new" };
        if (!validConditions.Contains(updateDto.Condition.ToLower()))
        {
            return (false, "Tình trạng sản phẩm không hợp lệ. Chọn: new, good, fair, hoặc poor");
        }

        // Handle images
        string? imageUrls = updateDto.ExistingImages;
        if (updateDto.NewImageFiles != null && updateDto.NewImageFiles.Any())
        {
            var newImages = await SaveProductImagesAsync(updateDto.NewImageFiles);
            imageUrls = string.IsNullOrEmpty(imageUrls) ? newImages : $"{imageUrls},{newImages}";
        }

        product.Name = updateDto.Name;
        product.Description = updateDto.Description;
        product.Price = updateDto.Price;
        product.Quantity = updateDto.Quantity;
        product.BatteryHealthPercent = updateDto.BatteryHealthPercent;
        product.Condition = updateDto.Condition;
        product.CategoryId = updateDto.CategoryId;
        product.Images = imageUrls;
        product.IsActive = updateDto.IsActive;
        
        // Auto-update IsSold status based on quantity
        if (product.Quantity <= 0)
        {
            product.IsSold = true;
            product.IsActive = false;
        }
        else if (product.IsSold == true && product.Quantity > 0)
        {
            // Re-enable if quantity is added back
            product.IsSold = false;
            product.IsActive = true;
        }
        
        product.UpdatedAt = DateTime.Now;

        await _productRepository.UpdateAsync(product);

        return (true, "Cập nhật sản phẩm thành công");
    }

    public async Task<(bool Success, string Message)> DeleteProductAsync(int productId, int sellerId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        
        if (product == null)
        {
            return (false, "Sản phẩm không tồn tại");
        }

        if (product.SellerId != sellerId)
        {
            return (false, "Bạn không có quyền xóa sản phẩm này");
        }

        await _productRepository.DeleteAsync(productId);

        return (true, "Xóa sản phẩm thành công");
    }

    public async Task<(bool Success, string Message)> DeactivateProductAsync(int productId, int sellerId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        
        if (product == null)
        {
            return (false, "Sản phẩm không tồn tại");
        }

        if (product.SellerId != sellerId)
        {
            return (false, "Bạn không có quyền thao tác sản phẩm này");
        }

        await _productRepository.DeactivateAsync(productId);

        return (true, "Đã ẩn sản phẩm");
    }

    public async Task<string> SaveProductImagesAsync(List<IFormFile> images)
    {
        var imageUrls = new List<string>();
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        foreach (var image in images)
        {
            if (image.Length > 0 && image.Length <= 5 * 1024 * 1024) // Max 5MB
            {
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                imageUrls.Add($"/uploads/products/{fileName}");
            }
        }

        return string.Join(",", imageUrls);
    }

    private ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            SellerId = product.SellerId,
            SellerName = product.Seller?.FullName ?? "Unknown",
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            BatteryHealthPercent = product.BatteryHealthPercent,
            Condition = product.Condition,
            Images = product.Images,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public async Task<IEnumerable<ProductDto>> GetPendingProductsAsync()
    {
        var products = await _productRepository.GetPendingProductsAsync();
        return products.Select(MapToProductDto);
    }

    public async Task<(bool Success, string Message)> ApproveProductAsync(int productId, int adminId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        
        if (product == null)
        {
            return (false, "Sản phẩm không tồn tại");
        }

        if (product.ApprovalStatus == "approved")
        {
            return (false, "Sản phẩm đã được duyệt");
        }

        product.ApprovalStatus = "approved";
        product.IsSold = false;
        product.ApprovedBy = adminId;
        product.ApprovedAt = DateTime.Now;
        product.RejectionReason = null;
        product.IsActive = true; // Public sản phẩm ngay khi duyệt

        await _productRepository.UpdateAsync(product);

        // Send real-time notification to seller
        await _notificationService.NotifyProductApprovalAsync(product.SellerId.Value, product.Id, product.Name, true);

        // Broadcast to ALL users - Product appears on homepage immediately
        var imageUrl = product.Images?.Split(',').FirstOrDefault() ?? "/images/no-image.png";
        await _notificationService.BroadcastNewProductAsync(
            product.Id,
            product.Name,
            product.Price,
            imageUrl
        );

        return (true, "Đã duyệt sản phẩm thành công. Sản phẩm đã được public!");
    }

    public async Task<(bool Success, string Message)> RejectProductAsync(int productId, int adminId, string reason)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        
        if (product == null)
        {
            return (false, "Sản phẩm không tồn tại");
        }

        if (product.ApprovalStatus == "rejected")
        {
            return (false, "Sản phẩm đã bị từ chối");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "Vui lòng nhập lý do từ chối");
        }

        product.ApprovalStatus = "rejected";
        product.ApprovedBy = adminId;
        product.ApprovedAt = DateTime.Now;
        product.RejectionReason = reason;
        product.IsActive = false;

        await _productRepository.UpdateAsync(product);

        // Send notification to seller
        await _notificationService.NotifyProductApprovalAsync(product.SellerId.Value, product.Id, product.Name, false);

        // Broadcast to ALL users - Product disappears from homepage immediately
        await _notificationService.BroadcastProductRemovedAsync(product.Id, reason);

        return (true, "Đã từ chối sản phẩm");
    }
}
