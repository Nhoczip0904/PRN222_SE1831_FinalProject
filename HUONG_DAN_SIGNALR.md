# Hướng dẫn SignalR Real-time Notifications

## 🔔 Tổng quan

SignalR đã được tích hợp vào toàn bộ website để gửi thông báo real-time cho người dùng.

## ✅ Các tính năng đã triển khai

### 1. **Notification Hub** (`/notificationHub`)
- Hub trung tâm xử lý tất cả thông báo
- Quản lý kết nối người dùng
- Hỗ trợ gửi thông báo đến:
  - Người dùng cụ thể
  - Tất cả người dùng
  - Nhóm Admin

### 2. **Notification Service**
Service quản lý tất cả loại thông báo:

#### **Duyệt sản phẩm**
- `NotifyProductApprovalAsync()` - Thông báo khi admin duyệt/từ chối sản phẩm
- Gửi đến: Người bán
- Loại: success (duyệt) / error (từ chối)

#### **Đơn hàng**
- `NotifyOrderStatusChangeAsync()` - Thông báo khi trạng thái đơn hàng thay đổi
- Gửi đến: Người mua + Người bán
- Các trạng thái: confirmed, shipped, delivered, cancelled

- `NotifyNewOrderAsync()` - Thông báo đơn hàng mới
- Gửi đến: Người bán
- Loại: success

#### **Hợp đồng**
- `NotifyContractApprovalAsync()` - Thông báo khi admin duyệt/từ chối hợp đồng
- Gửi đến: Người mua + Người bán
- Loại: success (duyệt) / error (từ chối)

#### **Đấu giá**
- `NotifyNewBidAsync()` - Thông báo có người đặt giá mới
- Gửi đến: Người bán
- Loại: info

- `NotifyAuctionWinnerAsync()` - Thông báo người thắng đấu giá
- Gửi đến: Người thắng
- Loại: success

#### **Thanh toán**
- `NotifyPaymentReceivedAsync()` - Thông báo nhận tiền từ đơn hàng
- Gửi đến: Người bán
- Loại: success

#### **Admin Notifications**
- `NotifyAdminNewProductAsync()` - Sản phẩm mới cần duyệt
- `NotifyAdminNewOrderAsync()` - Đơn hàng mới
- `NotifyAdminNewContractAsync()` - Hợp đồng mới cần duyệt
- Gửi đến: Tất cả Admin
- Loại: warning / info

### 3. **UI Components**

#### **Notification Bell** (Navbar)
- Icon chuông với badge đếm số thông báo chưa đọc
- Dropdown hiển thị danh sách thông báo
- Nút "Xóa tất cả" để xóa thông báo

#### **Toast Notifications**
- Hiển thị ở góc trên bên phải
- Tự động ẩn sau 5 giây
- Có link để chuyển đến trang liên quan
- Màu sắc theo loại:
  - 🟢 Success (xanh lá)
  - 🔴 Error (đỏ)
  - 🟡 Warning (vàng)
  - 🔵 Info (xanh dương)

#### **Notification List**
- Lưu trữ tối đa 50 thông báo
- Hiển thị thời gian nhận
- Click để đánh dấu đã đọc
- Badge cập nhật số lượng chưa đọc

### 4. **Sound Effect**
- Phát âm thanh khi có thông báo mới
- File: `/sounds/notification.mp3`
- Volume: 30%

## 📝 Cách sử dụng

### **Cho Developer - Gửi thông báo**

```csharp
// Inject INotificationService
private readonly INotificationService _notificationService;

// Ví dụ: Thông báo duyệt sản phẩm
await _notificationService.NotifyProductApprovalAsync(
    sellerId: 123,
    productId: 456,
    productName: "Pin Tesla Model 3",
    approved: true
);

// Ví dụ: Thông báo đơn hàng mới
await _notificationService.NotifyNewOrderAsync(
    sellerId: 123,
    orderId: 789,
    amount: 10000000
);

// Ví dụ: Thông báo admin
await _notificationService.NotifyAdminNewProductAsync(
    productId: 456,
    productName: "Pin Tesla Model 3",
    sellerName: "Nguyễn Văn A"
);
```

### **Cho User - Nhận thông báo**

1. **Đăng nhập vào hệ thống**
2. **Notification bell** sẽ tự động kết nối SignalR
3. **Khi có thông báo mới:**
   - Toast hiển thị ở góc trên bên phải
   - Badge chuông tăng số lượng
   - Âm thanh thông báo phát ra
   - Thông báo được thêm vào danh sách

4. **Xem thông báo:**
   - Click vào icon chuông
   - Xem danh sách thông báo
   - Click vào thông báo để đi đến trang liên quan
   - Thông báo tự động đánh dấu đã đọc

5. **Xóa thông báo:**
   - Click "Xóa tất cả" trong dropdown

## 🔧 Cấu hình kỹ thuật

### **Program.cs**
```csharp
// Đăng ký SignalR
builder.Services.AddSignalR();

// Đăng ký NotificationService
builder.Services.AddScoped<INotificationService, NotificationService>();

// Map Hub endpoint
app.MapHub<NotificationHub>("/notificationHub");
```

### **Layout.cshtml**
```html
<!-- Meta tag cho user role -->
<meta name="user-role" content="@(currentUser?.Role ?? "")" />

<!-- SignalR Client Library -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.0/signalr.min.js"></script>

<!-- Notification Script -->
<script src="~/js/notification.js"></script>
```

### **JavaScript Client**
```javascript
// Kết nối SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

// Nhận thông báo
connection.on("ReceiveNotification", (message, type, link) => {
    showNotification(message, type, link);
});

// Bắt đầu kết nối
await connection.start();
```

## 🎯 Các điểm tích hợp

### **1. ProductService**
- ✅ `ApproveProductAsync()` - Gửi thông báo khi duyệt
- ✅ `RejectProductAsync()` - Gửi thông báo khi từ chối

### **2. OrderService** (Cần tích hợp)
- ⏳ `CreateOrderFromCartAsync()` - Gửi thông báo đơn hàng mới
- ⏳ `UpdateOrderStatusAsync()` - Gửi thông báo thay đổi trạng thái

### **3. ContractService** (Cần tích hợp)
- ⏳ `ApproveContractAsync()` - Gửi thông báo duyệt hợp đồng
- ⏳ `RejectContractAsync()` - Gửi thông báo từ chối hợp đồng

### **4. AuctionService** (Cần tích hợp)
- ⏳ `PlaceBidAsync()` - Gửi thông báo đặt giá mới
- ⏳ `SelectWinnerAsync()` - Gửi thông báo người thắng

### **5. DeliveryService** (Cần tích hợp)
- ⏳ `ConfirmDeliveryAsync()` - Gửi thông báo nhận tiền

## 📊 Luồng hoạt động

### **Ví dụ: Duyệt sản phẩm**

```
1. Admin vào /Admin/Products/Details?id=123
2. Admin bấm "Duyệt sản phẩm"
3. ProductService.ApproveProductAsync() được gọi
4. Cập nhật database: approval_status = 'approved'
5. NotificationService.NotifyProductApprovalAsync() được gọi
6. SignalR gửi notification đến người bán (sellerId)
7. Client nhận notification qua connection.on("ReceiveNotification")
8. Toast hiển thị: "Sản phẩm 'Pin Tesla' đã được duyệt!"
9. Badge chuông tăng lên
10. Âm thanh thông báo phát ra
11. Notification được thêm vào danh sách
```

### **Ví dụ: Đơn hàng mới**

```
1. Người mua đặt hàng thành công
2. OrderService.CreateOrderFromCartAsync() tạo đơn
3. NotificationService.NotifyNewOrderAsync() được gọi
4. Gửi đến người bán: "Bạn có đơn hàng mới #789 - 10,000,000 đ"
5. NotificationService.NotifyAdminNewOrderAsync() được gọi
6. Gửi đến tất cả admin: "Đơn hàng mới #789 - 10,000,000 đ"
7. Cả người bán và admin đều nhận thông báo real-time
```

## 🚀 Mở rộng

### **Thêm loại thông báo mới**

1. **Thêm method vào INotificationService:**
```csharp
Task NotifyCustomEventAsync(int userId, string message);
```

2. **Implement trong NotificationService:**
```csharp
public async Task NotifyCustomEventAsync(int userId, string message)
{
    var connectionId = NotificationHub.GetConnectionId(userId);
    if (connectionId != null)
    {
        await _hubContext.Clients.Client(connectionId).SendAsync(
            "ReceiveNotification", 
            message, 
            "info",
            "/CustomPage"
        );
    }
}
```

3. **Gọi từ service:**
```csharp
await _notificationService.NotifyCustomEventAsync(userId, "Custom message");
```

## ⚠️ Lưu ý

1. **SignalR chỉ hoạt động khi user đã đăng nhập**
2. **Thông báo chỉ gửi đến user đang online**
3. **Offline users sẽ không nhận được thông báo**
4. **Cần implement database storage nếu muốn lưu lịch sử thông báo**
5. **Admin phải join "Admins" group để nhận thông báo admin**

## 🔍 Troubleshooting

### **Không nhận được thông báo?**
1. Kiểm tra Console: `SignalR Connected`
2. Kiểm tra user đã đăng nhập chưa
3. Kiểm tra connectionId có tồn tại không
4. Kiểm tra method gửi notification có được gọi không

### **Toast không hiển thị?**
1. Kiểm tra Bootstrap đã load chưa
2. Kiểm tra `toast-container` có được tạo không
3. Kiểm tra CSS của toast

### **Badge không cập nhật?**
1. Kiểm tra element `#notification-badge` có tồn tại không
2. Kiểm tra function `updateNotificationBadge()` có được gọi không

## 📱 Responsive

- Notification bell responsive trên mobile
- Toast position điều chỉnh theo màn hình
- Dropdown notification có scroll khi quá nhiều
