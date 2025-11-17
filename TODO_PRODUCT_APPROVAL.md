# ✅ HOÀN THÀNH HỆ THỐNG PHÊ DUYỆT SẢN PHẨM

## 📋 Đã làm xong

### 1. Database
- ✅ Tạo SQL script: `AddProductApproval.sql`
- ✅ Thêm cột: `approval_status`, `approved_by`, `approved_at`, `rejection_reason`

### 2. Backend
- ✅ Entity: `Product.cs` - Thêm các trường approval
- ✅ DbContext: `EvBatteryTrading2Context.cs` - Mapping columns
- ✅ Repository: `IProductRepository.cs` - Thêm methods
- ✅ Repository: `ProductRepository.cs` - Implement logic
- ✅ Service: `IProductService.cs` - Thêm methods
- ✅ Service: `ProductService.cs` - Implement approve/reject

---

## 🔧 CẦN LÀM TIẾP

### 1. Chạy SQL Script
```bash
# Mở SQL Server Management Studio
# Connect to: (local)
# Database: ev_battery_trading2
# Chạy file: AddProductApproval.sql
```

### 2. Tạo Admin Pages

#### File 1: `Admin/Products/Pending.cshtml`
```cshtml
@page
@model PRN222_FinalProject.Pages.Admin.Products.PendingModel
@{
    ViewData["Title"] = "Sản phẩm chờ duyệt";
}

<div class="container mt-4">
    <h2><i class="bi bi-clock-history"></i> Sản phẩm chờ duyệt</h2>
    
    @if (TempData["SuccessMessage"] != null)
    {
        <div class="alert alert-success">@TempData["SuccessMessage"]</div>
    }
    
    @if (Model.Products != null && Model.Products.Any())
    {
        <div class="row">
            @foreach (var product in Model.Products)
            {
                <div class="col-md-4 mb-4">
                    <div class="card">
                        <img src="@(product.Images?.Split(',').FirstOrDefault() ?? "/images/no-image.png")" 
                             class="card-img-top" style="height: 200px; object-fit: cover;">
                        <div class="card-body">
                            <h5>@product.Name</h5>
                            <p class="text-muted">Seller: @product.SellerName</p>
                            <p><strong>@product.Price.ToString("N0") VND</strong></p>
                            <p class="small">@product.CreatedAt?.ToString("dd/MM/yyyy HH:mm")</p>
                            
                            <div class="btn-group w-100">
                                <form method="post" asp-page-handler="Approve" asp-route-id="@product.Id" class="w-50">
                                    <button type="submit" class="btn btn-success w-100">
                                        <i class="bi bi-check-circle"></i> Duyệt
                                    </button>
                                </form>
                                <button type="button" class="btn btn-danger w-50" 
                                        onclick="showRejectModal(@product.Id, '@product.Name')">
                                    <i class="bi bi-x-circle"></i> Từ chối
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            }
        </div>
    }
    else
    {
        <div class="alert alert-info">Không có sản phẩm nào chờ duyệt</div>
    }
</div>

<!-- Modal từ chối -->
<div class="modal fade" id="rejectModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <form method="post" asp-page-handler="Reject">
                <div class="modal-header">
                    <h5 class="modal-title">Từ chối sản phẩm</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="rejectProductId" name="productId" />
                    <p>Sản phẩm: <strong id="rejectProductName"></strong></p>
                    <div class="mb-3">
                        <label class="form-label">Lý do từ chối *</label>
                        <textarea class="form-control" name="reason" rows="3" required></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                    <button type="submit" class="btn btn-danger">Xác nhận từ chối</button>
                </div>
            </form>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        function showRejectModal(productId, productName) {
            document.getElementById('rejectProductId').value = productId;
            document.getElementById('rejectProductName').textContent = productName;
            new bootstrap.Modal(document.getElementById('rejectModal')).show();
        }
    </script>
}
```

#### File 2: `Admin/Products/Pending.cshtml.cs`
```csharp
using BLL.Helpers;
using BLL.Services;
using DAL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_FinalProject.Pages.Admin.Products;

public class PendingModel : PageModel
{
    private readonly IProductService _productService;

    public PendingModel(IProductService productService)
    {
        _productService = productService;
    }

    public IEnumerable<ProductDto>? Products { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        Products = await _productService.GetPendingProductsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _productService.ApproveProductAsync(id, currentUser.Id);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int productId, string reason)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        
        if (currentUser == null || currentUser.Role != "admin")
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _productService.RejectProductAsync(productId, currentUser.Id, reason);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage();
    }
}
```

### 3. Cập nhật Navigation (_Layout.cshtml)
```cshtml
<!-- Thêm vào menu Admin -->
@if (currentUser?.Role == "admin")
{
    <li class="nav-item">
        <a class="nav-link text-white" asp-page="/Admin/Products/Pending">
            <i class="bi bi-clock-history"></i> Sản phẩm chờ duyệt
        </a>
    </li>
}
```

### 4. Cập nhật MyProducts.cshtml
```cshtml
<!-- Hiển thị trạng thái approval -->
@if (product.ApprovalStatus == "pending")
{
    <span class="badge bg-warning">Chờ duyệt</span>
}
else if (product.ApprovalStatus == "approved")
{
    <span class="badge bg-success">Đã duyệt</span>
}
else if (product.ApprovalStatus == "rejected")
{
    <span class="badge bg-danger">Bị từ chối</span>
    @if (!string.IsNullOrEmpty(product.RejectionReason))
    {
        <p class="text-danger small mt-2">
            <strong>Lý do:</strong> @product.RejectionReason
        </p>
    }
}
```

### 5. Cập nhật ProductDto.cs
```csharp
public string? ApprovalStatus { get; set; }
public int? ApprovedBy { get; set; }
public DateTime? ApprovedAt { get; set; }
public string? RejectionReason { get; set; }
```

### 6. Cập nhật MapToProductDto trong ProductService
```csharp
ApprovalStatus = product.ApprovalStatus,
ApprovedBy = product.ApprovedBy,
ApprovedAt = product.ApprovedAt,
RejectionReason = product.RejectionReason,
```

---

## 🚀 Cách chạy

### Bước 1: Chạy SQL
```sql
-- AddProductApproval.sql
```

### Bước 2: Tạo Pages
```
- Admin/Products/Pending.cshtml
- Admin/Products/Pending.cshtml.cs
```

### Bước 3: Cập nhật DTO
```
- ProductDto.cs
- ProductService.MapToProductDto()
```

### Bước 4: Cập nhật UI
```
- _Layout.cshtml (thêm link)
- MyProducts.cshtml (hiển thị trạng thái)
```

### Bước 5: Build & Run
```bash
dotnet build
dotnet run --project PRN222_FinalProject
```

---

## 🧪 Test

### Test 1: Tạo sản phẩm mới
1. Đăng nhập seller
2. Tạo sản phẩm mới
3. Kiểm tra: Không hiển thị trên trang chủ
4. Kiểm tra: Có badge "Chờ duyệt" trong MyProducts

### Test 2: Admin duyệt
1. Đăng nhập admin
2. Vào "Sản phẩm chờ duyệt"
3. Click "Duyệt"
4. Kiểm tra: Sản phẩm hiển thị trên trang chủ

### Test 3: Admin từ chối
1. Đăng nhập admin
2. Vào "Sản phẩm chờ duyệt"
3. Click "Từ chối", nhập lý do
4. Kiểm tra: Seller thấy lý do trong MyProducts

---

## ✅ Checklist

- [x] SQL script
- [x] Entity Product
- [x] DbContext mapping
- [x] Repository interface
- [x] Repository implementation
- [x] Service interface
- [x] Service implementation
- [ ] ProductDto update
- [ ] Admin Pending page
- [ ] Navigation link
- [ ] MyProducts status display
- [ ] Test đầy đủ

---

## 📝 Ghi chú

- Sản phẩm cũ sẽ tự động được set `approved` khi chạy SQL script
- Chỉ sản phẩm `approved` mới hiển thị cho người dùng
- Admin phải nhập lý do khi từ chối
- Seller có thể xem lý do từ chối trong MyProducts
