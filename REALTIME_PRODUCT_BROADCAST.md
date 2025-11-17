# Real-time Product Broadcast với SignalR

## 🎯 Mục tiêu

Khi admin duyệt sản phẩm → Sản phẩm xuất hiện **TỰ ĐỘNG** trên trang chủ của **TẤT CẢ người dùng** đang online **KHÔNG CẦN REFRESH**.

## 🔄 Luồng hoạt động

```
1. Admin duyệt sản phẩm
   ↓
2. ProductService.ApproveProductAsync()
   ├── IsActive = true
   ├── ApprovalStatus = "approved"
   ├── Notify seller (personal)
   └── BROADCAST to ALL users (public)
       ↓
3. SignalR Hub → Clients.All.SendAsync("NewProductAvailable")
   ↓
4. JavaScript client nhận event
   ├── Show notification toast
   └── Add product card to homepage (if on homepage)
       ↓
5. Sản phẩm xuất hiện NGAY LẬP TỨC với animation
```

## 💻 Implementation

### **1. Interface (BLL)**

```csharp
// BLL/Services/INotificationService.cs
public interface INotificationService
{
    // ... other methods
    
    Task BroadcastNewProductAsync(
        int productId, 
        string productName, 
        decimal price, 
        string imageUrl
    );
}
```

### **2. Implementation (Presentation)**

```csharp
// PRN222_FinalProject/Services/NotificationService.cs
public async Task BroadcastNewProductAsync(
    int productId, 
    string productName, 
    decimal price, 
    string imageUrl)
{
    // Broadcast to ALL connected clients
    await _hubContext.Clients.All.SendAsync(
        "NewProductAvailable",
        new
        {
            productId,
            productName,
            price,
            imageUrl,
            message = $"Sản phẩm mới: {productName} - {price:N0} đ"
        }
    );
}
```

### **3. Call khi duyệt sản phẩm**

```csharp
// BLL/Services/ProductService.cs
public async Task<(bool Success, string Message)> ApproveProductAsync(...)
{
    // ... update database
    
    // 1. Notify seller (personal)
    await _notificationService.NotifyProductApprovalAsync(
        product.SellerId.Value, 
        product.Id, 
        product.Name, 
        true
    );
    
    // 2. Broadcast to ALL users (public)
    var imageUrl = product.Images?.Split(',').FirstOrDefault() 
        ?? "/images/no-image.png";
        
    await _notificationService.BroadcastNewProductAsync(
        product.Id,
        product.Name,
        product.Price,
        imageUrl
    );
    
    return (true, "Đã duyệt sản phẩm thành công. Sản phẩm đã được public!");
}
```

### **4. JavaScript Client**

```javascript
// wwwroot/js/notification.js

// Listen for new product broadcast
connection.on("NewProductAvailable", (product) => {
    console.log("New product available:", product);
    
    // Show notification toast
    showNotification(
        product.message, 
        "info", 
        `/Products/Details?id=${product.productId}`
    );
    
    // Add product to homepage if user is on homepage
    if (window.location.pathname === '/' || 
        window.location.pathname === '/Index') {
        addProductToHomepage(product);
    }
});

// Add product card dynamically
function addProductToHomepage(product) {
    const productGrid = document.querySelector('.product-grid, .row.g-4');
    
    if (!productGrid) return;
    
    const productCard = `
        <div class="col-md-4 col-lg-3 product-item" 
             data-product-id="${product.productId}" 
             style="animation: fadeInUp 0.5s;">
            <div class="card h-100 shadow-sm">
                <img src="${product.imageUrl}" 
                     class="card-img-top" 
                     alt="${product.productName}" 
                     style="height: 200px; object-fit: cover;">
                <div class="card-body">
                    <h5 class="card-title">${product.productName}</h5>
                    <p class="card-text text-danger fw-bold">
                        ${product.price.toLocaleString('vi-VN')} đ
                    </p>
                    <span class="badge bg-success mb-2">
                        <i class="bi bi-star-fill"></i> MỚI
                    </span>
                    <a href="/Products/Details?id=${product.productId}" 
                       class="btn btn-primary w-100">
                        <i class="bi bi-eye"></i> Xem chi tiết
                    </a>
                </div>
            </div>
        </div>
    `;
    
    // Add to beginning with animation
    productGrid.insertAdjacentHTML('afterbegin', productCard);
}
```

## 🎨 Animation

```css
@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(20px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}
```

## 📊 Flow Diagram

```
┌─────────────┐                    ┌─────────────┐
│   Admin     │                    │ Người mua A │
└──────┬──────┘                    └──────┬──────┘
       │                                  │
       │ 1. Duyệt sản phẩm               │
       │                                  │
       ├──────────────────────────────────┤
       │   SignalR Broadcast (All)        │
       │   "NewProductAvailable"          │
       ├──────────────────────────────────┤
       │                                  │
       │                    2. 🔔 Notification
       │                    "Sản phẩm mới: Pin Tesla"
       │                                  │
       │                    3. ✨ Product card xuất hiện
       │                    (Không cần refresh)
       │                                  │
┌──────▼──────┐                    ┌──────▼──────┐
│ Người mua B │                    │ Người mua C │
│ (cũng nhận) │                    │ (cũng nhận) │
└─────────────┘                    └─────────────┘
```

## 🎯 User Experience

### **Người mua đang xem trang chủ:**

1. **Đang scroll xem sản phẩm**
2. **Admin duyệt sản phẩm mới**
3. **🔔 Toast notification xuất hiện:**
   ```
   ℹ️ Sản phẩm mới: Pin Tesla Model 3 - 10,000,000 đ
   ```
4. **✨ Product card xuất hiện ở đầu trang với animation**
   - Fade in từ dưới lên
   - Badge "MỚI" màu xanh
   - Không cần refresh page
5. **Click vào sản phẩm để xem chi tiết**

### **Người mua KHÔNG ở trang chủ:**

1. **Đang ở trang khác (ví dụ: /Cart)**
2. **Admin duyệt sản phẩm mới**
3. **🔔 Toast notification xuất hiện:**
   ```
   ℹ️ Sản phẩm mới: Pin Tesla Model 3 - 10,000,000 đ
   ```
4. **Click vào notification → Chuyển đến trang chi tiết**
5. **Hoặc quay lại trang chủ → Thấy sản phẩm mới**

## 🔍 Technical Details

### **SignalR Event:**

**Event name:** `NewProductAvailable`

**Payload:**
```json
{
    "productId": 123,
    "productName": "Pin Tesla Model 3",
    "price": 10000000,
    "imageUrl": "/uploads/products/tesla-123.jpg",
    "message": "Sản phẩm mới: Pin Tesla Model 3 - 10,000,000 đ"
}
```

### **Broadcast vs Personal Notification:**

| Type | Method | Receiver | Use Case |
|------|--------|----------|----------|
| **Personal** | `Clients.Client(connectionId)` | 1 user | Notify seller |
| **Group** | `Clients.Group("Admins")` | Admin group | Admin notifications |
| **Broadcast** | `Clients.All` | ALL users | New product |

### **Performance Considerations:**

1. **Broadcast chỉ gửi metadata** (id, name, price, image)
   - Không gửi toàn bộ HTML
   - Client tự render HTML

2. **Chỉ add product nếu đang ở homepage**
   - Check `window.location.pathname`
   - Tránh add vào page không cần

3. **Animation lightweight**
   - CSS animation (không dùng jQuery)
   - Duration: 0.5s

## ✅ Testing

### **Test Case 1: Người mua ở trang chủ**

1. Mở 2 browser:
   - Browser A: Login as Admin
   - Browser B: Login as Buyer (hoặc không login)
2. Browser B: Vào trang chủ
3. Browser A: Duyệt sản phẩm
4. **Expected:** Browser B thấy:
   - Toast notification
   - Product card xuất hiện ở đầu trang
   - Animation fade in

### **Test Case 2: Nhiều người mua cùng lúc**

1. Mở 3+ browsers
2. Tất cả vào trang chủ
3. Admin duyệt sản phẩm
4. **Expected:** TẤT CẢ browsers đều thấy sản phẩm mới

### **Test Case 3: Người mua không ở trang chủ**

1. Browser B: Vào trang /Cart
2. Admin duyệt sản phẩm
3. **Expected:** Browser B thấy:
   - Toast notification
   - Không add product card (vì không ở homepage)
4. Browser B: Quay lại trang chủ
5. **Expected:** Thấy sản phẩm mới (từ database)

## 🚀 Benefits

1. **Real-time Experience**
   - Không cần refresh
   - Sản phẩm xuất hiện ngay lập tức

2. **Better UX**
   - Người mua biết có sản phẩm mới
   - Tăng engagement

3. **Competitive Advantage**
   - Sản phẩm hot được thông báo ngay
   - Người mua nhanh tay hơn

4. **Scalable**
   - SignalR handle nhiều connections
   - Broadcast efficient

## 📝 Summary

| Feature | Description |
|---------|-------------|
| **Event** | `NewProductAvailable` |
| **Trigger** | Admin approve product |
| **Receiver** | ALL connected users |
| **Action** | Show notification + Add product card |
| **Animation** | Fade in from bottom |
| **Badge** | "MỚI" (green) |

**Key Point:** Sản phẩm xuất hiện TỰ ĐỘNG trên trang chủ của TẤT CẢ người dùng đang online mà KHÔNG CẦN REFRESH! 🚀
