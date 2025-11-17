# Luồng duyệt sản phẩm với SignalR Real-time

## 🔄 Luồng hoạt động

### **1. Người bán đăng tin**

```
Người bán → Tạo sản phẩm mới
    ↓
ProductService.CreateProductAsync()
    ├── IsActive = false (Ẩn sản phẩm)
    ├── ApprovalStatus = "pending"
    ├── Lưu vào database
    └── Gửi notification đến Admin (Real-time)
        ↓
Admin nhận thông báo ngay lập tức:
"Sản phẩm mới 'Pin Tesla Model 3' từ Nguyễn Văn A cần duyệt"
```

### **2. Admin duyệt sản phẩm**

```
Admin → Bấm "Duyệt sản phẩm"
    ↓
ProductService.ApproveProductAsync()
    ├── ApprovalStatus = "approved"
    ├── IsActive = true (Public sản phẩm)
    ├── Lưu vào database
    └── Gửi notification đến Người bán (Real-time)
        ↓
Người bán nhận thông báo ngay lập tức:
"Sản phẩm 'Pin Tesla Model 3' đã được duyệt!"
    ↓
Sản phẩm xuất hiện trên trang chủ ngay lập tức
```

## 📊 Trạng thái sản phẩm

| Trạng thái | IsActive | ApprovalStatus | Hiển thị | Ai thấy |
|------------|----------|----------------|----------|---------|
| **Mới tạo** | `false` | `pending` | ❌ Ẩn | Chỉ người bán & admin |
| **Đã duyệt** | `true` | `approved` | ✅ Public | Tất cả mọi người |
| **Bị từ chối** | `false` | `rejected` | ❌ Ẩn | Chỉ người bán & admin |

## 🔔 Notifications Real-time

### **Khi người bán đăng tin:**

**Gửi đến:** Admin (tất cả)

**Nội dung:**
```
🟡 Sản phẩm mới 'Pin Tesla Model 3' từ Nguyễn Văn A cần duyệt
Type: warning
Link: /Admin/Products/Details?id=123
```

**Code:**
```csharp
await _notificationService.NotifyAdminNewProductAsync(
    productId, 
    productName, 
    sellerName
);
```

### **Khi admin duyệt:**

**Gửi đến:** Người bán (sellerId)

**Nội dung:**
```
🟢 Sản phẩm 'Pin Tesla Model 3' đã được duyệt!
Type: success
Link: /Products/Details?id=123
```

**Code:**
```csharp
await _notificationService.NotifyProductApprovalAsync(
    sellerId, 
    productId, 
    productName, 
    approved: true
);
```

### **Khi admin từ chối:**

**Gửi đến:** Người bán (sellerId)

**Nội dung:**
```
🔴 Sản phẩm 'Pin Tesla Model 3' đã bị từ chối.
Type: error
Link: /Products/Details?id=123
```

**Code:**
```csharp
await _notificationService.NotifyProductApprovalAsync(
    sellerId, 
    productId, 
    productName, 
    approved: false
);
```

## 💻 Code Implementation

### **1. Tạo sản phẩm mới (ProductService.cs)**

```csharp
public async Task<(bool Success, string Message, int? ProductId)> CreateProductAsync(...)
{
    var product = new Product
    {
        // ... other fields
        IsActive = false,              // ← Ẩn sản phẩm
        ApprovalStatus = "pending",    // ← Chờ duyệt
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now
    };

    var createdProduct = await _productRepository.CreateAsync(product);

    // Notify admin (Real-time)
    await _notificationService.NotifyAdminNewProductAsync(
        createdProduct.Id, 
        createdProduct.Name, 
        seller.FullName
    );

    return (true, "Đăng sản phẩm thành công. Đang chờ admin duyệt.", createdProduct.Id);
}
```

### **2. Duyệt sản phẩm (ProductService.cs)**

```csharp
public async Task<(bool Success, string Message)> ApproveProductAsync(int productId, int adminId)
{
    var product = await _productRepository.GetByIdAsync(productId);
    
    product.ApprovalStatus = "approved";
    product.ApprovedBy = adminId;
    product.ApprovedAt = DateTime.Now;
    product.IsActive = true;           // ← Public sản phẩm

    await _productRepository.UpdateAsync(product);

    // Notify seller (Real-time)
    await _notificationService.NotifyProductApprovalAsync(
        product.SellerId.Value, 
        product.Id, 
        product.Name, 
        approved: true
    );

    return (true, "Đã duyệt sản phẩm thành công. Sản phẩm đã được public!");
}
```

## 🎯 User Experience

### **Người bán:**

1. **Đăng sản phẩm**
   - Điền form và submit
   - Thấy message: "Đăng sản phẩm thành công. Đang chờ admin duyệt."
   - Sản phẩm xuất hiện trong "Sản phẩm của tôi" với badge "Chờ duyệt"

2. **Chờ duyệt**
   - Có thể xem sản phẩm của mình
   - Sản phẩm KHÔNG hiển thị trên trang chủ
   - Đợi notification từ admin

3. **Nhận notification (Real-time)**
   - 🔔 Toast popup: "Sản phẩm 'Pin Tesla Model 3' đã được duyệt!"
   - Badge chuông tăng lên
   - Click vào notification → Chuyển đến trang chi tiết sản phẩm

4. **Sản phẩm được public**
   - Sản phẩm xuất hiện trên trang chủ ngay lập tức
   - Người mua có thể tìm thấy và mua

### **Admin:**

1. **Nhận notification (Real-time)**
   - 🔔 Toast popup: "Sản phẩm mới 'Pin Tesla Model 3' từ Nguyễn Văn A cần duyệt"
   - Badge chuông tăng lên
   - Click vào notification → Chuyển đến trang duyệt sản phẩm

2. **Duyệt sản phẩm**
   - Vào `/Admin/Products/Details?id=123`
   - Xem chi tiết sản phẩm
   - Bấm "Duyệt" hoặc "Từ chối"

3. **Kết quả**
   - Thấy message: "Đã duyệt sản phẩm thành công. Sản phẩm đã được public!"
   - Người bán nhận notification ngay lập tức
   - Sản phẩm xuất hiện trên trang chủ

## 🔍 Query sản phẩm

### **Trang chủ (Public)**
```csharp
// Chỉ hiển thị sản phẩm đã duyệt
var products = await _productRepository.GetAllAsync();
var publicProducts = products.Where(p => 
    p.IsActive == true && 
    p.ApprovalStatus == "approved"
);
```

### **Sản phẩm của tôi (Người bán)**
```csharp
// Hiển thị tất cả sản phẩm của người bán
var myProducts = await _productRepository.GetBySellerIdAsync(sellerId);
// Bao gồm cả pending, approved, rejected
```

### **Admin - Quản lý sản phẩm**
```csharp
// Hiển thị sản phẩm chờ duyệt
var pendingProducts = await _productRepository.GetPendingProductsAsync();
// ApprovalStatus == "pending"
```

## ⚡ Real-time Flow Diagram

```
┌─────────────┐                    ┌─────────────┐
│ Người bán   │                    │   Admin     │
└──────┬──────┘                    └──────┬──────┘
       │                                  │
       │ 1. Đăng sản phẩm                │
       │────────────────────────────────>│
       │                                  │
       │                    2. 🔔 Notification
       │                    "Sản phẩm mới cần duyệt"
       │                                  │
       │                                  │ 3. Duyệt
       │                                  │
       │ 4. 🔔 Notification               │
       │ "Sản phẩm đã được duyệt!"       │
       │<────────────────────────────────│
       │                                  │
       │ 5. Sản phẩm public ngay lập tức │
       │                                  │
┌──────▼──────┐                    ┌──────▼──────┐
│ Trang chủ   │◄───────────────────│ Database    │
│ (Updated)   │  IsActive = true   │ (Updated)   │
└─────────────┘                    └─────────────┘
```

## 📱 UI Changes

### **Trang "Sản phẩm của tôi"**

Hiển thị badge trạng thái:

```html
<div class="product-card">
    <h5>Pin Tesla Model 3</h5>
    
    @if (product.ApprovalStatus == "pending")
    {
        <span class="badge bg-warning">⏳ Chờ duyệt</span>
    }
    else if (product.ApprovalStatus == "approved")
    {
        <span class="badge bg-success">✅ Đã duyệt</span>
    }
    else if (product.ApprovalStatus == "rejected")
    {
        <span class="badge bg-danger">❌ Bị từ chối</span>
    }
</div>
```

### **Admin Dashboard**

Hiển thị số lượng sản phẩm chờ duyệt:

```html
<div class="card">
    <div class="card-body">
        <h5>Sản phẩm chờ duyệt</h5>
        <h2>@Model.PendingProductsCount</h2>
        <a href="/Admin/Products?status=pending">Xem tất cả</a>
    </div>
</div>
```

## ✅ Summary

| Bước | Actor | Action | Notification | Result |
|------|-------|--------|--------------|--------|
| 1 | Người bán | Đăng sản phẩm | → Admin | IsActive=false, pending |
| 2 | Admin | Nhận thông báo | - | Xem sản phẩm |
| 3 | Admin | Duyệt | → Người bán | IsActive=true, approved |
| 4 | Người bán | Nhận thông báo | - | Sản phẩm public |
| 5 | Tất cả | - | - | Thấy sản phẩm trên trang chủ |

**Key Points:**
- ✅ Sản phẩm mới bị ẩn (`IsActive = false`)
- ✅ Admin nhận notification real-time khi có sản phẩm mới
- ✅ Khi duyệt → `IsActive = true` → Public ngay lập tức
- ✅ Người bán nhận notification real-time khi được duyệt
- ✅ Không cần refresh page, tất cả real-time qua SignalR
