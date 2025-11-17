# 🔍 HỆ THỐNG PHÊ DUYỆT SẢN PHẨM

## 🎯 Yêu cầu

**Seller tạo sản phẩm → Admin phê duyệt → Hiển thị cho người dùng**

---

## 📋 Luồng hoạt động

### 1. Seller tạo sản phẩm
```
1. Seller đăng nhập
2. Vào "Sản phẩm của tôi" → "Đăng sản phẩm mới"
3. Điền thông tin sản phẩm
4. Click "Đăng sản phẩm"
5. ✅ Sản phẩm được tạo với trạng thái: "pending"
6. ❌ Sản phẩm KHÔNG hiển thị cho người dùng
```

### 2. Admin duyệt sản phẩm
```
1. Admin đăng nhập
2. Vào "Quản lý sản phẩm" → "Chờ duyệt"
3. Xem danh sách sản phẩm pending
4. Chọn hành động:
   - ✅ Duyệt → Sản phẩm hiển thị
   - ❌ Từ chối → Nhập lý do
```

### 3. Sản phẩm được duyệt
```
- Trạng thái: "approved"
- Hiển thị trên trang chủ
- Người dùng có thể mua/đấu giá
```

### 4. Sản phẩm bị từ chối
```
- Trạng thái: "rejected"
- KHÔNG hiển thị
- Seller xem được lý do từ chối
```

---

## 🗄️ Database Changes

### Bảng products - Thêm cột mới:
```sql
approval_status NVARCHAR(20) DEFAULT 'pending'
  -- Giá trị: 'pending', 'approved', 'rejected'

approved_by INT NULL
  -- FK → users.id (admin duyệt)

approved_at DATETIME NULL
  -- Thời gian duyệt

rejection_reason NVARCHAR(500) NULL
  -- Lý do từ chối (nếu rejected)
```

### Chạy SQL Script:
```sql
-- File: AddProductApproval.sql
-- Chạy script này để thêm các cột mới
```

---

## 💻 Code Changes

### 1. Entity: Product.cs
```csharp
public string? ApprovalStatus { get; set; } // pending, approved, rejected
public int? ApprovedBy { get; set; }
public DateTime? ApprovedAt { get; set; }
public string? RejectionReason { get; set; }
public virtual User? Approver { get; set; }
```

### 2. Repository: IProductRepository.cs
```csharp
Task<IEnumerable<Product>> GetApprovedProductsAsync();
Task<IEnumerable<Product>> GetPendingProductsAsync();
```

### 3. Service: IProductService.cs
```csharp
Task<IEnumerable<ProductDto>> GetPendingProductsAsync();
Task<(bool Success, string Message)> ApproveProductAsync(int productId, int adminId);
Task<(bool Success, string Message)> RejectProductAsync(int productId, int adminId, string reason);
```

### 4. Service: ProductService.cs
```csharp
// Lấy sản phẩm chờ duyệt
public async Task<IEnumerable<ProductDto>> GetPendingProductsAsync()
{
    var products = await _productRepository.GetPendingProductsAsync();
    return products.Select(MapToProductDto);
}

// Duyệt sản phẩm
public async Task<(bool Success, string Message)> ApproveProductAsync(int productId, int adminId)
{
    var product = await _productRepository.GetByIdAsync(productId);
    
    if (product == null)
        return (false, "Sản phẩm không tồn tại");
    
    if (product.ApprovalStatus == "approved")
        return (false, "Sản phẩm đã được duyệt");
    
    product.ApprovalStatus = "approved";
    product.ApprovedBy = adminId;
    product.ApprovedAt = DateTime.Now;
    product.RejectionReason = null;
    
    await _productRepository.UpdateAsync(product);
    
    return (true, "Đã duyệt sản phẩm thành công");
}

// Từ chối sản phẩm
public async Task<(bool Success, string Message)> RejectProductAsync(int productId, int adminId, string reason)
{
    var product = await _productRepository.GetByIdAsync(productId);
    
    if (product == null)
        return (false, "Sản phẩm không tồn tại");
    
    if (string.IsNullOrWhiteSpace(reason))
        return (false, "Vui lòng nhập lý do từ chối");
    
    product.ApprovalStatus = "rejected";
    product.ApprovedBy = adminId;
    product.ApprovedAt = DateTime.Now;
    product.RejectionReason = reason;
    product.IsActive = false;
    
    await _productRepository.UpdateAsync(product);
    
    return (true, "Đã từ chối sản phẩm");
}
```

### 5. Repository: ProductRepository.cs
```csharp
// Chỉ lấy sản phẩm đã duyệt
public async Task<IEnumerable<Product>> GetActiveProductsAsync()
{
    return await _context.Products
        .Include(p => p.Seller)
        .Include(p => p.Category)
        .Where(p => p.IsActive && p.ApprovalStatus == "approved")
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();
}

// Lấy sản phẩm chờ duyệt
public async Task<IEnumerable<Product>> GetPendingProductsAsync()
{
    return await _context.Products
        .Include(p => p.Seller)
        .Include(p => p.Category)
        .Where(p => p.ApprovalStatus == "pending")
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();
}
```

---

## 🎨 UI Pages (Cần tạo)

### 1. Admin/Products/Pending.cshtml
```
Trang danh sách sản phẩm chờ duyệt cho admin
- Hiển thị thông tin sản phẩm
- Nút "Duyệt"
- Nút "Từ chối" (popup nhập lý do)
```

### 2. Products/MyProducts.cshtml (Cập nhật)
```
Thêm hiển thị trạng thái approval:
- pending: Badge màu vàng "Chờ duyệt"
- approved: Badge màu xanh "Đã duyệt"
- rejected: Badge màu đỏ "Bị từ chối" + lý do
```

---

## ⚠️ Lưu ý quan trọng

### 1. Sản phẩm mới tạo
- ✅ Trạng thái mặc định: "pending"
- ❌ KHÔNG hiển thị cho người dùng
- ✅ Seller vẫn thấy trong "Sản phẩm của tôi"

### 2. Chỉ sản phẩm "approved" hiển thị
- ✅ Trang chủ
- ✅ Danh sách sản phẩm
- ✅ Tìm kiếm
- ✅ Có thể mua/đấu giá

### 3. Admin có quyền
- ✅ Xem tất cả sản phẩm
- ✅ Duyệt sản phẩm
- ✅ Từ chối sản phẩm (phải nhập lý do)

### 4. Seller
- ✅ Xem trạng thái sản phẩm của mình
- ✅ Xem lý do từ chối (nếu bị từ chối)
- ❌ Không thể tự duyệt

---

## 🧪 Test Cases

### Test 1: Tạo sản phẩm mới
```
1. Seller tạo sản phẩm
2. Kiểm tra: approval_status = 'pending'
3. Kiểm tra: Không hiển thị trên trang chủ
4. Kiểm tra: Seller thấy trong "Sản phẩm của tôi"
```

### Test 2: Admin duyệt
```
1. Admin vào "Chờ duyệt"
2. Click "Duyệt" sản phẩm
3. Kiểm tra: approval_status = 'approved'
4. Kiểm tra: Hiển thị trên trang chủ
5. Kiểm tra: Có thể mua/đấu giá
```

### Test 3: Admin từ chối
```
1. Admin vào "Chờ duyệt"
2. Click "Từ chối"
3. Nhập lý do: "Hình ảnh không rõ ràng"
4. Kiểm tra: approval_status = 'rejected'
5. Kiểm tra: is_active = false
6. Kiểm tra: Seller thấy lý do từ chối
```

### Test 4: Sản phẩm cũ
```
1. Chạy SQL script
2. Kiểm tra: Sản phẩm cũ tự động approved
3. Kiểm tra: Vẫn hiển thị bình thường
```

---

## 🚀 Cách triển khai

### Bước 1: Chạy SQL Script
```sql
-- File: AddProductApproval.sql
-- Thêm các cột mới vào bảng products
-- Cập nhật sản phẩm cũ thành 'approved'
```

### Bước 2: Cập nhật Code
```
✅ Entity: Product.cs
✅ DbContext: EvBatteryTrading2Context.cs
✅ Repository: IProductRepository.cs, ProductRepository.cs
✅ Service: IProductService.cs, ProductService.cs
```

### Bước 3: Tạo UI Pages
```
- Admin/Products/Pending.cshtml
- Admin/Products/Pending.cshtml.cs
- Cập nhật Products/MyProducts.cshtml
```

### Bước 4: Test
```
1. Dừng VS
2. Build: dotnet build
3. Run: dotnet run
4. Test luồng đầy đủ
```

---

## ✅ Tổng kết

### Luồng hoàn chỉnh:
```
1. Seller tạo sản phẩm → pending
2. Admin xem danh sách chờ duyệt
3. Admin duyệt/từ chối
4. Nếu duyệt → Hiển thị cho người dùng
5. Nếu từ chối → Seller xem lý do
```

### Ưu điểm:
- ✅ Kiểm soát chất lượng sản phẩm
- ✅ Tránh spam/lừa đảo
- ✅ Minh bạch với seller
- ✅ Dễ quản lý

### Bảo mật:
- ✅ Chỉ admin mới duyệt được
- ✅ Phải nhập lý do khi từ chối
- ✅ Lưu lịch sử phê duyệt
