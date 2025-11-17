# Phân tích 7 Bước Nghiệp vụ Logic - Hiện trạng & Kế hoạch

## 📊 Tổng quan Hiện trạng

| Bước | Tính năng | Trạng thái | Cần bổ sung |
|------|-----------|------------|-------------|
| 1 | Tìm kiếm & Lọc sản phẩm | ⚠️ Cơ bản | Lọc nâng cao, AI gợi ý |
| 2 | Theo dõi & So sánh | ⚠️ Một phần | Wishlist, Chat |
| 3 | Quyết định giao dịch | ✅ Đầy đủ | Đấu giá + Mua ngay đã có |
| 4 | Thanh toán & Ký hợp đồng | ✅ Đầy đủ | VNPay + Contract đã có |
| 5 | Nhận hàng & Xác nhận | ⚠️ Cơ bản | Xác nhận nhận hàng, chuyển tiền |
| 6 | Đánh giá & Phản hồi | ❌ Chưa có | Cần tạo mới hoàn toàn |
| 7 | Hậu mãi & Hỗ trợ | ❌ Chưa có | Cần tạo mới hoàn toàn |

---

## 🔍 BƯỚC 1: Tìm kiếm & Lọc sản phẩm

### ✅ Đã có:
- [x] Tìm kiếm theo keyword
- [x] Lọc theo category
- [x] Lọc theo khoảng giá (min/max)
- [x] Sắp xếp (newest, price_asc, price_desc)
- [x] Pagination

### ❌ Cần bổ sung:
- [ ] **Lọc nâng cao:**
  - Tình trạng (mới, cũ, đã qua sử dụng)
  - Dung lượng pin (kWh)
  - Số km đã đi
  - Năm sản xuất
  - Đời xe
  - Khoảng cách địa lý (location-based)
  
- [ ] **Uy tín người bán:**
  - Hiển thị rating người bán
  - Lọc theo rating tối thiểu
  - Badge "Người bán uy tín"

- [ ] **AI/Gợi ý thông minh:**
  - Sản phẩm tương tự
  - Dựa trên lịch sử xem
  - Trending products
  - Recently viewed

### 📝 Implementation Plan:
1. Thêm fields vào ProductSearchDto
2. Update ProductRepository.SearchAsync()
3. Thêm seller rating display
4. Tạo RecommendationService (optional)

---

## ❤️ BƯỚC 2: Theo dõi & So sánh

### ✅ Đã có:
- [x] So sánh sản phẩm (Compare) - Đã có page

### ❌ Cần bổ sung:
- [ ] **Wishlist/Favorites:**
  - Bảng `favorites` (user_id, product_id, created_at)
  - Nút "Thêm vào yêu thích" ❤️
  - Trang "Sản phẩm yêu thích"
  - Badge số lượng wishlist

- [ ] **Chat nội bộ:**
  - Bảng `messages` (sender_id, receiver_id, product_id, content, created_at)
  - Chat realtime (SignalR hoặc polling)
  - Inbox/Outbox
  - Notification khi có tin nhắn mới

- [ ] **Enhanced Compare:**
  - So sánh chi tiết hơn (specs, battery, price history)
  - Export comparison to PDF
  - Share comparison link

### 📝 Implementation Plan:
1. Tạo Favorites system (DB + Service + Pages)
2. Tạo Messaging system (DB + Service + Pages)
3. Enhance Compare page

---

## 💰 BƯỚC 3: Quyết định giao dịch

### ✅ Đã có:
- [x] Hệ thống đấu giá (Auction)
  - Bảng auctions, bids
  - Đặt giá thầu
  - Tự động cập nhật
  - Kết thúc đấu giá
  
- [x] Mua ngay
  - Add to cart
  - Checkout
  - Order creation

- [x] Wallet system
  - Nạp tiền
  - Giữ tiền tạm thời

### ⚠️ Cần cải thiện:
- [ ] **Escrow (Tạm giữ tiền):**
  - Khi mua ngay → tự động hold tiền trong wallet
  - Chỉ chuyển cho seller khi buyer xác nhận nhận hàng
  - Hoàn tiền nếu hủy giao dịch

### 📝 Implementation Plan:
1. Thêm trạng thái "held" cho wallet transactions
2. Auto-hold money khi order created
3. Release money khi order delivered & confirmed

---

## 💳 BƯỚC 4: Thanh toán & Ký hợp đồng

### ✅ Đã có - HOÀN CHỈNH:
- [x] VNPay integration
- [x] Wallet payment
- [x] Contract system
  - Tự động tạo hợp đồng
  - Xác nhận 2 bên
  - Admin duyệt
  - Hợp đồng PDF

### ✔️ Không cần bổ sung - Đã đầy đủ!

---

## 📦 BƯỚC 5: Nhận hàng & Xác nhận

### ✅ Đã có:
- [x] Order status tracking
- [x] Seller update status (confirmed → shipped)

### ❌ Cần bổ sung:
- [ ] **Buyer xác nhận nhận hàng:**
  - Nút "Đã nhận hàng" cho buyer
  - Order status: shipped → delivered
  - Trigger: Chuyển tiền cho seller

- [ ] **Kiểm tra hàng hóa:**
  - Checkbox "Hàng đúng mô tả"
  - Upload ảnh khi nhận hàng (optional)
  - Báo cáo vấn đề nếu có

- [ ] **Auto-release money:**
  - Sau X ngày tự động xác nhận nếu buyer không phản hồi
  - Chuyển tiền từ escrow → seller wallet

### 📝 Implementation Plan:
1. Thêm button "Đã nhận hàng" trong Order details
2. Update OrderService.ConfirmDeliveryAsync()
3. Trigger WalletService.ReleaseFunds()
4. Add auto-confirm job (background service)

---

## ⭐ BƯỚC 6: Đánh giá & Phản hồi

### ❌ Chưa có - CẦN TẠO MỚI:

#### Database Schema:
```sql
-- Bảng reviews (Đánh giá)
CREATE TABLE reviews (
    id INT PRIMARY KEY IDENTITY(1,1),
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    buyer_id INT NOT NULL,
    seller_id INT NOT NULL,
    
    -- Đánh giá sản phẩm
    product_rating INT NOT NULL, -- 1-5 sao
    product_review NVARCHAR(1000) NULL,
    
    -- Đánh giá người bán
    seller_rating INT NOT NULL, -- 1-5 sao
    seller_review NVARCHAR(1000) NULL,
    
    -- Ảnh đánh giá
    images NVARCHAR(MAX) NULL, -- JSON array
    
    -- Phản hồi từ seller
    seller_response NVARCHAR(1000) NULL,
    seller_response_at DATETIME NULL,
    
    is_verified BIT DEFAULT 0, -- Đã mua hàng thật
    is_helpful_count INT DEFAULT 0,
    
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_reviews_order FOREIGN KEY (order_id) REFERENCES orders(id),
    CONSTRAINT FK_reviews_product FOREIGN KEY (product_id) REFERENCES products(id),
    CONSTRAINT FK_reviews_buyer FOREIGN KEY (buyer_id) REFERENCES users(id),
    CONSTRAINT FK_reviews_seller FOREIGN KEY (seller_id) REFERENCES users(id)
);

-- Bảng seller_ratings (Tổng hợp rating người bán)
CREATE TABLE seller_ratings (
    id INT PRIMARY KEY IDENTITY(1,1),
    seller_id INT NOT NULL UNIQUE,
    total_reviews INT DEFAULT 0,
    average_rating DECIMAL(3,2) DEFAULT 0,
    five_star_count INT DEFAULT 0,
    four_star_count INT DEFAULT 0,
    three_star_count INT DEFAULT 0,
    two_star_count INT DEFAULT 0,
    one_star_count INT DEFAULT 0,
    updated_at DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_seller_ratings_user FOREIGN KEY (seller_id) REFERENCES users(id)
);
```

#### Features cần tạo:
- [ ] ReviewService (CRUD reviews)
- [ ] Trang đánh giá sau khi nhận hàng
- [ ] Hiển thị reviews trong product details
- [ ] Seller có thể phản hồi review
- [ ] Tính toán average rating tự động
- [ ] Badge "Verified Purchase"
- [ ] Helpful button cho reviews

### 📝 Implementation Plan:
1. Tạo database tables
2. Tạo Review entity & ReviewService
3. Tạo page đánh giá (/Orders/Review/{orderId})
4. Hiển thị reviews trong product details
5. Seller response page
6. Calculate & update seller ratings

---

## 🛠️ BƯỚC 7: Hậu mãi & Hỗ trợ

### ❌ Chưa có - CẦN TẠO MỚI:

#### Database Schema:
```sql
-- Bảng support_tickets (Khiếu nại/Hỗ trợ)
CREATE TABLE support_tickets (
    id INT PRIMARY KEY IDENTITY(1,1),
    ticket_number VARCHAR(50) NOT NULL UNIQUE,
    user_id INT NOT NULL,
    order_id INT NULL,
    product_id INT NULL,
    
    category VARCHAR(50) NOT NULL, -- product_issue, delivery_issue, payment_issue, other
    subject NVARCHAR(200) NOT NULL,
    description NVARCHAR(2000) NOT NULL,
    images NVARCHAR(MAX) NULL, -- JSON array
    
    status VARCHAR(20) DEFAULT 'open', -- open, in_progress, resolved, closed
    priority VARCHAR(20) DEFAULT 'normal', -- low, normal, high, urgent
    
    assigned_to INT NULL, -- Admin ID
    admin_notes NVARCHAR(2000) NULL,
    resolution NVARCHAR(2000) NULL,
    resolved_at DATETIME NULL,
    
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_tickets_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT FK_tickets_order FOREIGN KEY (order_id) REFERENCES orders(id),
    CONSTRAINT FK_tickets_product FOREIGN KEY (product_id) REFERENCES products(id),
    CONSTRAINT FK_tickets_admin FOREIGN KEY (assigned_to) REFERENCES users(id)
);

-- Bảng ticket_messages (Tin nhắn trong ticket)
CREATE TABLE ticket_messages (
    id INT PRIMARY KEY IDENTITY(1,1),
    ticket_id INT NOT NULL,
    user_id INT NOT NULL,
    message NVARCHAR(1000) NOT NULL,
    attachments NVARCHAR(MAX) NULL,
    is_admin BIT DEFAULT 0,
    created_at DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_ticket_messages_ticket FOREIGN KEY (ticket_id) REFERENCES support_tickets(id),
    CONSTRAINT FK_ticket_messages_user FOREIGN KEY (user_id) REFERENCES users(id)
);

-- Bảng transaction_history (Lịch sử giao dịch để thống kê)
-- Đã có trong wallet_transactions, có thể tái sử dụng
```

#### Features cần tạo:
- [ ] SupportTicketService
- [ ] Trang tạo ticket hỗ trợ
- [ ] Danh sách tickets của user
- [ ] Admin ticket management
- [ ] Ticket conversation/messaging
- [ ] Email notification khi ticket update
- [ ] Dashboard thống kê (admin)
- [ ] Export reports

### 📝 Implementation Plan:
1. Tạo database tables
2. Tạo SupportTicket entity & Service
3. User pages:
   - Tạo ticket mới
   - Danh sách tickets
   - Chi tiết ticket & chat
4. Admin pages:
   - Danh sách tất cả tickets
   - Assign tickets
   - Resolve tickets
   - Dashboard & reports

---

## 📋 Priority Implementation Order

### Phase 1: Critical (Cần ngay)
1. ✅ **Bước 4** - Đã có đầy đủ
2. ⚠️ **Bước 5** - Xác nhận nhận hàng & chuyển tiền
3. ⚠️ **Bước 3** - Escrow/Hold money

### Phase 2: Important (Quan trọng)
4. ❌ **Bước 6** - Review & Rating system
5. ⚠️ **Bước 2** - Wishlist & Messaging
6. ⚠️ **Bước 1** - Advanced filters

### Phase 3: Nice to have (Bổ sung)
7. ❌ **Bước 7** - Support tickets
8. 🤖 **AI Recommendations**
9. 📊 **Analytics & Reports**

---

## 🎯 Quick Win Features (Triển khai nhanh)

### 1. Wishlist/Favorites (2-3 hours)
- Simple table + CRUD
- Heart icon on products
- Favorites page

### 2. Confirm Delivery (1-2 hours)
- Button "Đã nhận hàng"
- Update order status
- Release funds

### 3. Basic Reviews (3-4 hours)
- Review form after delivery
- Display reviews on product
- Calculate average rating

### 4. Advanced Search Filters (2-3 hours)
- Add more filter fields
- Update search logic
- UI improvements

---

## 📊 Estimated Timeline

| Phase | Features | Time | Priority |
|-------|----------|------|----------|
| Phase 1 | Delivery confirmation + Escrow | 1-2 days | 🔴 Critical |
| Phase 2 | Reviews + Wishlist | 2-3 days | 🟡 Important |
| Phase 3 | Advanced filters + Messaging | 2-3 days | 🟡 Important |
| Phase 4 | Support tickets + Dashboard | 3-4 days | 🟢 Nice to have |

**Total:** ~8-12 days for complete 7-step business logic

---

## 🚀 Next Actions

1. ✅ Tạo file phân tích này
2. ⏭️ Bắt đầu Phase 1: Delivery confirmation
3. ⏭️ Implement escrow/hold money
4. ⏭️ Create review system
5. ⏭️ Add wishlist feature
6. ⏭️ Enhance search filters
7. ⏭️ Build support ticket system

---

## 📝 Notes

- Hệ thống đã có nền tảng tốt (Auction, Payment, Contract)
- Cần tập trung vào user experience (Reviews, Wishlist, Support)
- Database schema đã được thiết kế sẵn
- Có thể triển khai từng phase độc lập
