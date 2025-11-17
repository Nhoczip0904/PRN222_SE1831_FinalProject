# 🚀 Roadmap Triển khai 7 Bước Nghiệp vụ Logic

## ✅ Đã Hoàn thành

### 1. Database Schema
- [x] Tạo file SQL: `CREATE_7_STEPS_FEATURES.sql`
- [x] Bảng `favorites` (Wishlist)
- [x] Bảng `messages` (Chat)
- [x] Bảng `reviews` và `seller_ratings` (Đánh giá)
- [x] Bảng `support_tickets` và `ticket_messages` (Hỗ trợ)
- [x] Thêm cột vào `orders` (delivery confirmation)
- [x] Thêm cột vào `products` (advanced search)

### 2. Entity Classes
- [x] `Favorite.cs`
- [x] `Message.cs`
- [x] `Review.cs` và `SellerRating.cs`
- [x] `SupportTicket.cs` và `TicketMessage.cs`
- [x] Cập nhật `Order.cs` (delivery fields)
- [x] Cập nhật `Product.cs` (advanced search fields)

### 3. DbContext
- [x] Thêm DbSet cho tất cả entities mới

---

## 🔄 Cần Triển khai

### PHASE 1: Core Features (Priority: 🔴 Critical)

#### 1.1. Delivery Confirmation Service
**File:** `BLL/Services/DeliveryService.cs`

```csharp
public interface IDeliveryService
{
    Task<(bool Success, string Message)> ConfirmDeliveryAsync(int orderId, int buyerId, string? notes);
    Task<bool> CanConfirmDeliveryAsync(int orderId, int buyerId);
}
```

**Features:**
- Buyer xác nhận đã nhận hàng
- Cập nhật order status: shipped → delivered
- Trigger release funds từ escrow
- Gửi notification cho seller

**Pages:**
- Thêm button "Đã nhận hàng" trong `/Orders/Index`
- Modal xác nhận với checkbox "Hàng đúng mô tả"

---

#### 1.2. Escrow/Hold Money Service
**File:** `BLL/Services/EscrowService.cs`

```csharp
public interface IEscrowService
{
    Task<(bool Success, string Message)> HoldFundsAsync(int orderId, int buyerId, decimal amount);
    Task<(bool Success, string Message)> ReleaseFundsAsync(int orderId);
    Task<(bool Success, string Message)> RefundFundsAsync(int orderId, string reason);
}
```

**Features:**
- Auto-hold money khi order created
- Release money khi delivery confirmed
- Refund nếu order cancelled
- Track escrow status

**Integration:**
- Modify `OrderService.CreateOrderFromCartAsync()`
- Modify `DeliveryService.ConfirmDeliveryAsync()`

---

### PHASE 2: User Experience (Priority: 🟡 Important)

#### 2.1. Wishlist/Favorites Feature
**Files:**
- `BLL/Services/FavoriteService.cs`
- `Pages/Favorites/Index.cshtml`

```csharp
public interface IFavoriteService
{
    Task<(bool Success, string Message)> AddToFavoritesAsync(int userId, int productId);
    Task<(bool Success, string Message)> RemoveFromFavoritesAsync(int userId, int productId);
    Task<List<Product>> GetUserFavoritesAsync(int userId);
    Task<bool> IsFavoriteAsync(int userId, int productId);
    Task<int> GetFavoriteCountAsync(int userId);
}
```

**Pages:**
- `/Favorites/Index` - Danh sách sản phẩm yêu thích
- Thêm heart icon ❤️ vào product cards
- Badge số lượng favorites trong navbar

---

#### 2.2. Review & Rating System
**Files:**
- `BLL/Services/ReviewService.cs`
- `Pages/Orders/Review.cshtml` - Trang đánh giá
- `Pages/Reviews/Index.cshtml` - Danh sách reviews của user

```csharp
public interface IReviewService
{
    Task<(bool Success, string Message)> CreateReviewAsync(CreateReviewDto dto);
    Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, UpdateReviewDto dto);
    Task<(bool Success, string Message)> AddSellerResponseAsync(int reviewId, int sellerId, string response);
    Task<List<Review>> GetProductReviewsAsync(int productId);
    Task<List<Review>> GetSellerReviewsAsync(int sellerId);
    Task<Review?> GetOrderReviewAsync(int orderId);
    Task<SellerRating?> GetSellerRatingAsync(int sellerId);
    Task<bool> CanReviewAsync(int orderId, int buyerId);
}
```

**Features:**
- Đánh giá sau khi nhận hàng
- Rating 1-5 sao cho sản phẩm và người bán
- Upload ảnh đánh giá
- Seller có thể phản hồi
- Hiển thị reviews trong product details
- Tính average rating tự động

**Pages:**
- `/Orders/Review/{orderId}` - Form đánh giá
- Update `/Products/Details` - Hiển thị reviews
- `/Reviews/MyReviews` - Reviews của tôi

---

#### 2.3. Messaging System
**Files:**
- `BLL/Services/MessageService.cs`
- `Pages/Messages/Index.cshtml` - Inbox
- `Pages/Messages/Conversation.cshtml` - Chi tiết conversation

```csharp
public interface IMessageService
{
    Task<(bool Success, string Message)> SendMessageAsync(SendMessageDto dto);
    Task<List<Message>> GetConversationAsync(int userId, int otherUserId);
    Task<List<Message>> GetInboxAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<(bool Success, string Message)> MarkAsReadAsync(int messageId, int userId);
}
```

**Features:**
- Chat giữa buyer và seller
- Inbox/Sent messages
- Unread badge
- Link từ product → "Liên hệ người bán"

---

### PHASE 3: Advanced Features (Priority: 🟢 Nice to have)

#### 3.1. Advanced Search & Filters
**Update:** `ProductSearchDto.cs` và `ProductRepository.cs`

```csharp
public class ProductSearchDto
{
    // Existing
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Condition { get; set; }
    public int? MinBatteryHealth { get; set; }
    
    // New advanced filters
    public decimal? MinBatteryCapacity { get; set; }
    public decimal? MaxBatteryCapacity { get; set; }
    public int? MaxMileage { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public string? Location { get; set; }
    public int? MinSellerRating { get; set; }
    
    public string? SortBy { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
```

**UI Updates:**
- Advanced filter panel (collapsible)
- Range sliders cho price, capacity, mileage
- Year dropdown
- Location autocomplete
- Seller rating filter

---

#### 3.2. Support Ticket System
**Files:**
- `BLL/Services/SupportTicketService.cs`
- `Pages/Support/Create.cshtml` - Tạo ticket
- `Pages/Support/Index.cshtml` - Danh sách tickets
- `Pages/Support/Details.cshtml` - Chi tiết & chat
- `Pages/Admin/Support/Index.cshtml` - Admin quản lý

```csharp
public interface ISupportTicketService
{
    Task<(bool Success, string Message, SupportTicket? Ticket)> CreateTicketAsync(CreateTicketDto dto);
    Task<List<SupportTicket>> GetUserTicketsAsync(int userId);
    Task<List<SupportTicket>> GetAllTicketsAsync(string? status, string? priority);
    Task<SupportTicket?> GetTicketByIdAsync(int ticketId);
    Task<(bool Success, string Message)> AddMessageAsync(int ticketId, int userId, string message, bool isAdmin);
    Task<(bool Success, string Message)> AssignTicketAsync(int ticketId, int adminId);
    Task<(bool Success, string Message)> UpdateStatusAsync(int ticketId, string status);
    Task<(bool Success, string Message)> ResolveTicketAsync(int ticketId, int adminId, string resolution);
}
```

**Features:**
- User tạo ticket hỗ trợ
- Phân loại: product_issue, delivery_issue, payment_issue, other
- Priority: low, normal, high, urgent
- Admin assign & resolve
- Conversation trong ticket
- Email notification

---

#### 3.3. Recommendation System (Optional - AI)
**File:** `BLL/Services/RecommendationService.cs`

```csharp
public interface IRecommendationService
{
    Task<List<Product>> GetSimilarProductsAsync(int productId, int count = 4);
    Task<List<Product>> GetRecommendedForUserAsync(int userId, int count = 8);
    Task<List<Product>> GetTrendingProductsAsync(int count = 10);
    Task<List<Product>> GetRecentlyViewedAsync(int userId, int count = 5);
    Task TrackViewAsync(int userId, int productId);
}
```

**Features:**
- Sản phẩm tương tự (based on category, price range)
- Gợi ý cho user (based on view history, favorites)
- Trending products (most viewed/favorited)
- Recently viewed

---

## 📋 Implementation Checklist

### Database
- [ ] Run `CREATE_7_STEPS_FEATURES.sql`
- [ ] Verify all tables created
- [ ] Test triggers and functions

### Backend Services
- [ ] DeliveryService
- [ ] EscrowService
- [ ] FavoriteService
- [ ] ReviewService
- [ ] MessageService
- [ ] SupportTicketService
- [ ] RecommendationService (optional)

### Frontend Pages
- [ ] Delivery confirmation button
- [ ] Favorites page
- [ ] Review form & display
- [ ] Messaging inbox
- [ ] Support ticket system
- [ ] Advanced search filters
- [ ] Seller rating display

### Integration
- [ ] Update OrderService for escrow
- [ ] Update Product details for reviews
- [ ] Update navbar for unread counts
- [ ] Email notifications

### Testing
- [ ] Test delivery confirmation flow
- [ ] Test escrow hold/release
- [ ] Test review creation & display
- [ ] Test messaging
- [ ] Test support tickets
- [ ] Test advanced search

---

## 🎯 Quick Start Guide

### Step 1: Setup Database
```sql
-- Run in SQL Server Management Studio
USE [EvBatteryTrading2]
GO

-- Execute the script
-- File: CREATE_7_STEPS_FEATURES.sql
```

### Step 2: Register Services
```csharp
// In Program.cs
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IEscrowService, EscrowService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
```

### Step 3: Build & Test
```bash
dotnet build
dotnet run --project PRN222_FinalProject
```

---

## 📊 Estimated Timeline

| Feature | Time | Priority |
|---------|------|----------|
| Delivery Confirmation | 2-3 hours | 🔴 Critical |
| Escrow System | 3-4 hours | 🔴 Critical |
| Wishlist | 2-3 hours | 🟡 Important |
| Reviews & Ratings | 4-5 hours | 🟡 Important |
| Messaging | 4-5 hours | 🟡 Important |
| Advanced Search | 2-3 hours | 🟡 Important |
| Support Tickets | 4-5 hours | 🟢 Nice to have |
| Recommendations | 3-4 hours | 🟢 Nice to have |

**Total:** 24-32 hours (~3-4 days)

---

## 🚦 Current Status

✅ **Completed:**
- Database schema designed
- Entity classes created
- DbContext updated
- Analysis & planning documents

⏳ **In Progress:**
- Service implementations
- Page creations
- UI/UX enhancements

❌ **Not Started:**
- Testing
- Documentation
- Deployment

---

## 📝 Next Actions

1. ✅ Run `CREATE_7_STEPS_FEATURES.sql`
2. ⏭️ Implement DeliveryService
3. ⏭️ Implement EscrowService
4. ⏭️ Implement FavoriteService
5. ⏭️ Implement ReviewService
6. ⏭️ Create user-facing pages
7. ⏭️ Test complete workflow

---

## 🎓 Learning Resources

- **Entity Framework Core:** https://docs.microsoft.com/ef/core/
- **ASP.NET Razor Pages:** https://docs.microsoft.com/aspnet/core/razor-pages/
- **Bootstrap 5:** https://getbootstrap.com/docs/5.0/
- **SQL Server:** https://docs.microsoft.com/sql/

---

## 💡 Tips

1. **Start Small:** Implement one feature at a time
2. **Test Often:** Test after each feature implementation
3. **Use Existing Code:** Reference Contract and Auction systems
4. **Keep It Simple:** Don't over-engineer
5. **User First:** Focus on user experience

---

## 🆘 Support

Nếu gặp vấn đề, tham khảo:
- `7_STEPS_BUSINESS_LOGIC_ANALYSIS.md` - Phân tích chi tiết
- `CREATE_7_STEPS_FEATURES.sql` - Database schema
- Existing services (ContractService, AuctionService) - Tham khảo code mẫu
