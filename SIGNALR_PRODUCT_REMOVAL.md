# SignalR Real-time Product Removal

## 🎯 Mục tiêu

Khi admin từ chối sản phẩm → Sản phẩm **BIẾN MẤT NGAY LẬP TỨC** trên màn hình của tất cả user đang xem **KHÔNG CẦN REFRESH**.

## 🔄 Luồng hoạt động

```
Admin từ chối sản phẩm
    ↓
ProductService.RejectProductAsync()
    ├── IsActive = false
    ├── ApprovalStatus = "rejected"
    ├── Notify seller (personal) 🔔
    └── BROADCAST to ALL users 📢
        ↓
SignalR → Clients.All.SendAsync("ProductRemoved")
    ↓
JavaScript client (TẤT CẢ người dùng)
    ├── Find product card by data-product-id
    ├── Add fade out animation
    └── Remove from DOM
        ↓
✨ Sản phẩm biến mất với animation fade-out
```

## 💻 Implementation

### **1. Interface (BLL)**

```csharp
// BLL/Services/INotificationService.cs
public interface INotificationService
{
    // ... other methods
    
    Task BroadcastProductRemovedAsync(int productId, string reason);
}
```

### **2. Implementation (Presentation)**

```csharp
// PRN222_FinalProject/Services/NotificationService.cs
public async Task BroadcastProductRemovedAsync(int productId, string reason)
{
    // Broadcast to ALL connected clients to remove product from UI
    await _hubContext.Clients.All.SendAsync(
        "ProductRemoved",
        new
        {
            productId,
            reason,
            message = $"Sản phẩm đã bị gỡ: {reason}"
        }
    );
}
```

### **3. Call khi từ chối sản phẩm**

```csharp
// BLL/Services/ProductService.cs
public async Task<(bool Success, string Message)> RejectProductAsync(...)
{
    // ... update database
    
    // 1. Notify seller (personal)
    await _notificationService.NotifyProductApprovalAsync(
        product.SellerId.Value, 
        product.Id, 
        product.Name, 
        false
    );
    
    // 2. Broadcast to ALL users (public)
    await _notificationService.BroadcastProductRemovedAsync(
        product.Id, 
        reason
    );
    
    return (true, "Đã từ chối sản phẩm");
}
```

### **4. HTML - Add data-product-id attribute**

```html
<!-- Pages/Products/Index.cshtml -->
@foreach (var product in Model.Products)
{
    <div class="col-md-3 mb-4" data-product-id="@product.Id">
        <div class="card h-100">
            <!-- Product content -->
        </div>
    </div>
}
```

**QUAN TRỌNG:** Phải thêm `data-product-id="@product.Id"` vào container div để JavaScript có thể tìm và xóa.

### **5. JavaScript Client**

```javascript
// wwwroot/js/notification.js

// Listen for product removed broadcast
connection.on("ProductRemoved", (data) => {
    console.log("Product removed:", data);
    
    // Remove product from page immediately
    removeProductFromPage(data.productId);
    
    // If user is viewing that product detail page, redirect
    if (window.location.pathname.includes(`/Products/Details`) && 
        window.location.search.includes(`id=${data.productId}`)) {
        showNotification("Sản phẩm này đã bị gỡ bởi admin", "error", "/Products");
        setTimeout(() => {
            window.location.href = "/Products";
        }, 2000);
    }
});

// Remove product from page with animation
function removeProductFromPage(productId) {
    const productCard = document.querySelector(`[data-product-id="${productId}"]`);
    
    if (productCard) {
        // Add fade out animation
        productCard.style.animation = 'fadeOut 0.5s';
        
        // Remove after animation
        setTimeout(() => {
            productCard.remove();
            console.log('Product removed from page:', productId);
        }, 500);
    }
}
```

### **6. CSS Animation**

```css
@keyframes fadeOut {
    from {
        opacity: 1;
        transform: scale(1);
    }
    to {
        opacity: 0;
        transform: scale(0.8);
    }
}
```

## 🎨 User Experience

### **User đang xem trang Products:**

1. **Đang scroll xem sản phẩm**
2. **Admin từ chối sản phẩm #123**
3. **✨ Product card #123 fade out và biến mất**
4. **Các sản phẩm khác tự động sắp xếp lại**
5. **KHÔNG CẦN REFRESH!**

### **User đang xem chi tiết sản phẩm bị từ chối:**

1. **Đang xem `/Products/Details?id=123`**
2. **Admin từ chối sản phẩm #123**
3. **🔔 Toast: "Sản phẩm này đã bị gỡ bởi admin"**
4. **Tự động redirect về `/Products` sau 2 giây**

### **User đang ở trang khác:**

1. **Đang ở trang `/Cart`**
2. **Admin từ chối sản phẩm**
3. **Không có gì xảy ra** (vì không có product card trên trang này)
4. **Khi quay lại `/Products` → Sản phẩm đã không còn**

## 📊 Flow Diagram

```
┌─────────────┐                    ┌─────────────┐
│   Admin     │                    │  User A     │
└──────┬──────┘                    └──────┬──────┘
       │                                  │
       │ 1. Từ chối sản phẩm #123        │ Đang xem Products
       │                                  │
       ├──────────────────────────────────┤
       │   SignalR Broadcast (All)        │
       │   "ProductRemoved"               │
       ├──────────────────────────────────┤
       │                                  │
       │                    2. ✨ Product #123 fade out
       │                    3. Remove from DOM
       │                    (Không cần refresh)
       │                                  │
┌──────▼──────┐                    ┌──────▼──────┐
│  User B     │                    │  User C     │
│ (cũng thấy) │                    │ (cũng thấy) │
└─────────────┘                    └─────────────┘
```

## 🔍 Technical Details

### **SignalR Event:**

**Event name:** `ProductRemoved`

**Payload:**
```json
{
    "productId": 123,
    "reason": "Vi phạm chính sách",
    "message": "Sản phẩm đã bị gỡ: Vi phạm chính sách"
}
```

### **DOM Selector:**

```javascript
// Find by data attribute
const productCard = document.querySelector(`[data-product-id="${productId}"]`);
```

**Yêu cầu:** HTML phải có `data-product-id` attribute:
```html
<div class="col-md-3 mb-4" data-product-id="123">
```

### **Animation Timeline:**

1. **0ms:** Receive SignalR event
2. **0ms:** Add `fadeOut` animation (duration: 500ms)
3. **500ms:** Remove element from DOM
4. **500ms+:** Browser reflows layout automatically

## ✅ Checklist

### **Files đã update:**

- [x] `BLL/Services/INotificationService.cs` - Add `BroadcastProductRemovedAsync()`
- [x] `PRN222_FinalProject/Services/NotificationService.cs` - Implement method
- [x] `BLL/Services/ProductService.cs` - Call broadcast on reject
- [x] `wwwroot/js/notification.js` - Add event handler + remove function
- [x] `Pages/Products/Index.cshtml` - Add `data-product-id` attribute

### **Cần kiểm tra thêm:**

- [ ] Trang chủ (nếu có list sản phẩm) - Add `data-product-id`
- [ ] Trang tìm kiếm (nếu có) - Add `data-product-id`
- [ ] Trang danh mục (nếu có) - Add `data-product-id`

## 🚀 Testing

### **Test Case 1: User xem trang Products**

**Steps:**
1. Mở 2 browsers
2. Browser A: Login as Admin
3. Browser B: Vào `/Products` (không login hoặc login as buyer)
4. Browser A: Từ chối sản phẩm #123
5. **Expected:** Browser B thấy product #123 fade out và biến mất

### **Test Case 2: User xem chi tiết sản phẩm bị từ chối**

**Steps:**
1. Browser B: Vào `/Products/Details?id=123`
2. Browser A: Từ chối sản phẩm #123
3. **Expected:** 
   - Toast notification: "Sản phẩm này đã bị gỡ bởi admin"
   - Redirect về `/Products` sau 2 giây

### **Test Case 3: Nhiều user cùng lúc**

**Steps:**
1. Mở 3+ browsers, tất cả vào `/Products`
2. Admin từ chối sản phẩm
3. **Expected:** TẤT CẢ browsers thấy sản phẩm biến mất

### **Test Case 4: User không ở trang Products**

**Steps:**
1. Browser B: Vào `/Cart`
2. Admin từ chối sản phẩm
3. **Expected:** Không có gì xảy ra (vì không có product card)
4. Browser B: Quay lại `/Products`
5. **Expected:** Sản phẩm đã không còn (từ database)

## 🐛 Troubleshooting

### **Lỗi: Product không biến mất**

**Nguyên nhân:** Không tìm thấy product card

**Debug:**
```javascript
// Check console
console.log('Product card not found on this page:', productId);
```

**Giải pháp:** Kiểm tra HTML có `data-product-id` attribute không

### **Lỗi: Animation không mượt**

**Nguyên nhân:** CSS animation chưa được load

**Giải pháp:** Function `removeProductFromPage()` tự động thêm CSS animation

### **Lỗi: Redirect loop trên detail page**

**Nguyên nhân:** Condition check URL không chính xác

**Giải pháp:** Check cả pathname và search params:
```javascript
if (window.location.pathname.includes(`/Products/Details`) && 
    window.location.search.includes(`id=${data.productId}`))
```

## 📝 Summary

| Feature | Description |
|---------|-------------|
| **Event** | `ProductRemoved` |
| **Trigger** | Admin reject product |
| **Receiver** | ALL connected users |
| **Action** | Fade out + Remove from DOM |
| **Animation** | 0.5s fade out + scale down |
| **Redirect** | Only if viewing detail page |

**Key Points:**
- ✅ Sản phẩm biến mất TỰ ĐỘNG trên tất cả màn hình
- ✅ KHÔNG CẦN REFRESH
- ✅ Animation mượt mà
- ✅ Tự động redirect nếu đang xem chi tiết
- ✅ Broadcast đến TẤT CẢ users (không chỉ seller)

**Rebuild và test - Sản phẩm sẽ biến mất real-time!** 🚀
