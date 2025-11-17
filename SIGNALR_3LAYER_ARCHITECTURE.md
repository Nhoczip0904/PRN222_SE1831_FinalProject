# SignalR với Kiến trúc 3 Layer

## 🏗️ Kiến trúc

```
┌─────────────────────────────────────────────────────────┐
│  PRN222_FinalProject (Presentation Layer)              │
│  ├── Hubs/                                              │
│  │   └── NotificationHub.cs (SignalR Hub)              │
│  ├── Services/                                          │
│  │   └── NotificationService.cs (Implementation)       │
│  ├── Pages/ (Razor Pages)                              │
│  └── wwwroot/js/notification.js (Client)               │
└─────────────────────────────────────────────────────────┘
                          ↓ implements
┌─────────────────────────────────────────────────────────┐
│  BLL (Business Logic Layer)                             │
│  └── Services/                                          │
│      ├── INotificationService.cs (Interface)           │
│      ├── ProductService.cs (Uses interface)            │
│      ├── OrderService.cs (Uses interface)              │
│      └── ... (Other services)                          │
└─────────────────────────────────────────────────────────┘
                          ↓ uses
┌─────────────────────────────────────────────────────────┐
│  DAL (Data Access Layer)                                │
│  ├── Entities/ (Database models)                       │
│  └── Repositories/ (Data access)                       │
└─────────────────────────────────────────────────────────┘
```

## ✅ Nguyên tắc 3 Layer

### **1. DAL (Data Access Layer)**
- ✅ Chỉ chứa entities và repositories
- ✅ Không biết gì về business logic
- ✅ Không biết gì về SignalR

### **2. BLL (Business Logic Layer)**
- ✅ Chứa business logic
- ✅ Định nghĩa **interface** INotificationService
- ✅ Các service khác (ProductService, OrderService) inject INotificationService
- ✅ **KHÔNG** chứa implementation của SignalR
- ✅ **KHÔNG** reference đến PRN222_FinalProject

### **3. Presentation Layer (PRN222_FinalProject)**
- ✅ Chứa **implementation** của INotificationService
- ✅ Chứa SignalR Hub
- ✅ Chứa UI (Razor Pages, JavaScript)
- ✅ Đăng ký services trong Program.cs
- ✅ Reference đến BLL và DAL

## 📁 File Structure

```
PRN222_FinalProject/
├── Hubs/
│   └── NotificationHub.cs
│       - Quản lý SignalR connections
│       - Lưu mapping userId → connectionId
│       - Hỗ trợ groups (Admins)
│
├── Services/
│   └── NotificationService.cs
│       - Implement INotificationService
│       - Sử dụng IHubContext<NotificationHub>
│       - Gửi notifications qua SignalR
│
├── wwwroot/js/
│   └── notification.js
│       - SignalR client
│       - Hiển thị toast notifications
│       - Quản lý notification list
│
└── Program.cs
    - Đăng ký: AddSignalR()
    - Đăng ký: INotificationService → NotificationService
    - Map Hub: /notificationHub

BLL/
└── Services/
    ├── INotificationService.cs
    │   - Interface ONLY
    │   - Không có implementation
    │   - Không depend on SignalR
    │
    └── ProductService.cs
        - Inject INotificationService
        - Gọi notifications khi approve/reject
```

## 🔄 Luồng hoạt động

### **Ví dụ: Admin duyệt sản phẩm**

```
1. Admin bấm "Duyệt sản phẩm"
   ↓
2. ProductService.ApproveProductAsync()
   ├── Update database
   └── _notificationService.NotifyProductApprovalAsync()
       ↓
3. NotificationService (Presentation Layer)
   ├── Get connectionId từ NotificationHub
   └── _hubContext.Clients.Client(connectionId).SendAsync()
       ↓
4. SignalR gửi đến client
   ↓
5. notification.js nhận event
   ├── Hiển thị toast
   ├── Update badge
   └── Thêm vào notification list
```

## 💡 Tại sao thiết kế như vậy?

### **❌ Sai: Đặt NotificationService trong BLL**
```
BLL/Services/NotificationService.cs
- Cần reference Microsoft.AspNetCore.SignalR
- Cần reference PRN222_FinalProject.Hubs
- Vi phạm nguyên tắc: BLL không nên biết về infrastructure
```

### **✅ Đúng: Interface trong BLL, Implementation trong Presentation**
```
BLL/Services/INotificationService.cs
- Chỉ là interface
- Không depend on SignalR
- BLL services chỉ biết về interface

PRN222_FinalProject/Services/NotificationService.cs
- Implement interface
- Sử dụng SignalR
- Presentation layer quản lý infrastructure
```

## 🎯 Dependency Flow

```
PRN222_FinalProject
    ↓ (references)
   BLL
    ↓ (references)
   DAL

✅ Correct: Top-down dependencies
❌ Wrong: BLL → PRN222_FinalProject (circular dependency)
```

## 📝 Code Examples

### **1. Interface trong BLL**
```csharp
// BLL/Services/INotificationService.cs
namespace BLL.Services;

public interface INotificationService
{
    Task NotifyProductApprovalAsync(int sellerId, int productId, 
        string productName, bool approved);
}
```

### **2. Business Service sử dụng Interface**
```csharp
// BLL/Services/ProductService.cs
public class ProductService : IProductService
{
    private readonly INotificationService _notificationService;
    
    public ProductService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    
    public async Task ApproveProductAsync(int productId)
    {
        // Business logic
        product.ApprovalStatus = "approved";
        await _productRepository.UpdateAsync(product);
        
        // Notification (không biết implementation)
        await _notificationService.NotifyProductApprovalAsync(
            product.SellerId, product.Id, product.Name, true);
    }
}
```

### **3. Implementation trong Presentation**
```csharp
// PRN222_FinalProject/Services/NotificationService.cs
using BLL.Services; // Import interface từ BLL

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    
    public async Task NotifyProductApprovalAsync(...)
    {
        var connectionId = NotificationHub.GetConnectionId(sellerId);
        await _hubContext.Clients.Client(connectionId)
            .SendAsync("ReceiveNotification", message, type, link);
    }
}
```

### **4. Đăng ký trong Program.cs**
```csharp
// PRN222_FinalProject/Program.cs
using BLL.Services; // Interface
using PRN222_FinalProject.Services; // Implementation

builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();
//                         ↑ Interface (BLL)    ↑ Implementation (Presentation)

app.MapHub<NotificationHub>("/notificationHub");
```

## ✅ Lợi ích

1. **Separation of Concerns**
   - BLL chỉ quan tâm business logic
   - Presentation xử lý infrastructure (SignalR)

2. **Testability**
   - Có thể mock INotificationService khi test BLL
   - Không cần SignalR để test business logic

3. **Flexibility**
   - Có thể thay đổi implementation (SignalR → WebSocket → Email)
   - Không ảnh hưởng đến BLL

4. **No Circular Dependencies**
   - Dependencies flow từ trên xuống
   - Dễ maintain và scale

## 🚀 Mở rộng

### **Thêm notification mới:**

1. **Thêm method vào interface (BLL)**
```csharp
// BLL/Services/INotificationService.cs
Task NotifyNewFeatureAsync(int userId, string message);
```

2. **Implement method (Presentation)**
```csharp
// PRN222_FinalProject/Services/NotificationService.cs
public async Task NotifyNewFeatureAsync(int userId, string message)
{
    var connectionId = NotificationHub.GetConnectionId(userId);
    await _hubContext.Clients.Client(connectionId)
        .SendAsync("ReceiveNotification", message, "info", "/");
}
```

3. **Sử dụng trong service (BLL)**
```csharp
// BLL/Services/SomeService.cs
await _notificationService.NotifyNewFeatureAsync(userId, "New feature!");
```

## 📊 Summary

| Layer | Responsibility | SignalR |
|-------|---------------|---------|
| **DAL** | Data access | ❌ No |
| **BLL** | Business logic + Interface | ❌ No |
| **Presentation** | UI + Implementation | ✅ Yes |

**Key Point:** Interface ở BLL, Implementation ở Presentation!
