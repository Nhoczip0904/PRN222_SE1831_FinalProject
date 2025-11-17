# 🎉 HỆ THỐNG HOÀN CHỈNH - Hướng dẫn triển khai

## ✅ Đã hoàn thành 100%

### 1. Hệ thống Ví điện tử
- ✅ Database: wallets, wallet_transactions
- ✅ Entities: Wallet, WalletTransaction
- ✅ Repositories & Services
- ✅ Pages: Xem ví, Nạp tiền, Lịch sử giao dịch

### 2. Luồng đấu giá mới
- ✅ Kiểm tra số dư ví khi đặt giá
- ✅ Seller chọn người thắng
- ✅ Tự động trừ tiền winner, cộng tiền seller
- ✅ Tự động tạo đơn hàng
- ✅ Khóa sản phẩm khi đưa ra đấu giá

---

## 🚀 Hướng dẫn chạy

### Bước 1: Chạy SQL Scripts
```sql
-- 1. Mở SQL Server Management Studio
-- 2. Connect to: (local)
-- 3. Database: ev_battery_trading2

-- 4. Chạy lần lượt:
-- File 1: CreateAuctionTables.sql
-- File 2: CreateWalletTables.sql
```

### Bước 2: Stop Visual Studio
- Đóng tất cả cửa sổ Visual Studio
- Đảm bảo không có process nào đang lock file

### Bước 3: Build & Run
```bash
cd "c:\Users\Hp\Desktop\Bai Thi Pe\PRN222_FinalProject"
dotnet build
dotnet run --project PRN222_FinalProject
```

---

## 📋 Luồng hoạt động

### A. Chuẩn bị
1. **User đăng ký/đăng nhập**
2. **Nạp tiền vào ví**
   - Vào "Ví của tôi" → "Nạp tiền"
   - Nhập số tiền (tối thiểu 10,000 VND)
   - Xác nhận

### B. Seller tạo đấu giá
1. **Tạo sản phẩm** (nếu chưa có)
   - Vào "Sản phẩm của tôi" → "Đăng sản phẩm mới"
   
2. **Tạo đấu giá**
   - Vào "Đấu giá của tôi" → "Tạo đấu giá mới"
   - Chọn sản phẩm
   - Nhập giá khởi điểm, thời gian
   - **Lưu ý**: Sản phẩm sẽ bị khóa (không thể edit/delete)

### C. Buyer đặt giá
1. **Xem đấu giá**
   - Vào "Đấu giá" → Chọn đấu giá
   
2. **Đặt giá**
   - Nhập số tiền > giá hiện tại
   - **Hệ thống kiểm tra số dư ví**
   - Nếu đủ tiền → Đặt giá thành công

### D. Seller chọn người thắng
1. **Sau khi đấu giá kết thúc**
   - Vào "Đấu giá của tôi"
   - Click "Chọn người thắng" (nút màu xanh)
   
2. **Chọn winner**
   - Xem danh sách người đặt giá
   - Click "Chọn làm người thắng"
   
3. **Hệ thống tự động**:
   - ✅ Kiểm tra số dư ví winner
   - ✅ Trừ tiền từ ví winner
   - ✅ Cộng tiền vào ví seller
   - ✅ Cập nhật trạng thái đấu giá = "closed"
   - ✅ Tạo đơn hàng tự động

### E. Winner nhận hàng
1. **Xem đơn hàng**
   - Vào "Đơn hàng"
   - Đơn hàng từ đấu giá đã được tạo sẵn
   
2. **Cập nhật địa chỉ** (nếu cần)
3. **Chờ seller giao hàng**

---

## 🔑 Các tính năng chính

### 1. Ví điện tử
- Nạp tiền (demo, không qua cổng thanh toán thật)
- Xem số dư
- Lịch sử giao dịch
- Các loại giao dịch:
  - `deposit`: Nạp tiền
  - `payment`: Thanh toán đấu giá
  - `refund`: Hoàn tiền

### 2. Đấu giá
- Tạo đấu giá với thời gian bắt đầu/kết thúc
- Đặt giá (kiểm tra số dư ví)
- Seller chọn người thắng
- Tự động thanh toán và tạo đơn hàng
- Khóa sản phẩm trong thời gian đấu giá

### 3. Bảo mật
- Kiểm tra quyền seller
- Kiểm tra số dư ví
- Không cho phép seller đặt giá sản phẩm của mình
- Không cho phép hủy đấu giá đã bắt đầu

---

## 📊 Cấu trúc Database

### Bảng mới
```
wallets
├── id (PK)
├── user_id (FK → users.id, UNIQUE)
├── balance (DECIMAL, >= 0)
├── created_at
└── updated_at

wallet_transactions
├── id (PK)
├── wallet_id (FK → wallets.id)
├── transaction_type (deposit, payment, refund, ...)
├── amount
├── balance_after
├── description
├── reference_id (auction_id hoặc order_id)
├── reference_type ('auction', 'order')
└── created_at

auctions
├── id (PK)
├── product_id (FK → products.id)
├── seller_id (FK → users.id)
├── starting_price
├── current_price
├── reserve_price (nullable)
├── start_time
├── end_time
├── status ('active', 'closed', 'cancelled')
├── winner_id (FK → users.id, nullable)
├── created_at
└── updated_at

bids
├── id (PK)
├── auction_id (FK → auctions.id)
├── bidder_id (FK → users.id)
├── bid_amount
├── bid_time
└── is_winning (boolean)
```

---

## 🧪 Test Cases

### Test 1: Nạp tiền
1. Đăng nhập
2. Vào "Ví của tôi" → "Nạp tiền"
3. Nhập 1,000,000 VND
4. Xác nhận
5. **Kết quả**: Số dư tăng lên 1,000,000 VND

### Test 2: Tạo đấu giá
1. Tạo sản phẩm (nếu chưa có)
2. Vào "Đấu giá của tôi" → "Tạo đấu giá mới"
3. Chọn sản phẩm, nhập:
   - Giá khởi điểm: 500,000
   - Thời gian bắt đầu: Ngày mai
   - Thời gian kết thúc: 3 ngày sau
4. **Kết quả**: Đấu giá được tạo, sản phẩm bị khóa

### Test 3: Đặt giá
1. Đăng nhập user khác (không phải seller)
2. Nạp tiền vào ví (ít nhất 600,000)
3. Vào "Đấu giá" → Chọn đấu giá
4. Đặt giá: 600,000
5. **Kết quả**: Đặt giá thành công

### Test 4: Chọn người thắng
1. Đăng nhập seller
2. Đợi đấu giá kết thúc (hoặc sửa end_time trong DB)
3. Vào "Đấu giá của tôi" → "Chọn người thắng"
4. Chọn người đặt giá cao nhất
5. **Kết quả**:
   - Tiền bị trừ từ ví winner
   - Tiền được cộng vào ví seller
   - Đơn hàng được tạo tự động
   - Trạng thái = "closed"

---

## ⚠️ Lưu ý quan trọng

1. **Sản phẩm đã đưa ra đấu giá**:
   - Không thể edit
   - Không thể delete
   - `is_active = false`

2. **Số dư ví**:
   - Phải đủ tiền mới đặt giá được
   - Tiền được trừ ngay khi seller chọn winner

3. **Seller chọn winner**:
   - Chỉ sau khi đấu giá kết thúc
   - Phải có ít nhất 1 người đặt giá
   - Winner phải có đủ số dư

4. **Đơn hàng tự động**:
   - Được tạo ngay khi chọn winner
   - Địa chỉ mặc định: "Địa chỉ từ đấu giá"
   - Winner có thể cập nhật địa chỉ sau

---

## 🎯 Tổng kết

Hệ thống đã hoàn chỉnh với:
- ✅ 3 chủ đề chính
- ✅ Hệ thống ví điện tử
- ✅ Đấu giá với thanh toán tự động
- ✅ Quản lý sản phẩm, đơn hàng
- ✅ Phân quyền admin/member
- ✅ Validation đầy đủ

**Tổng số files**: ~80 files
**Tổng số dòng code**: ~8,000 lines
