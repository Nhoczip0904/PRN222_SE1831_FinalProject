# Debug: Tạo đấu giá không được

## Các bước kiểm tra

### 1. Kiểm tra database đã có bảng chưa
```sql
-- Mở SQL Server Management Studio
-- Chạy query này:
USE ev_battery_trading2;
GO

SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('auctions', 'bids');
```

**Kết quả mong đợi**: Phải có 2 bảng `auctions` và `bids`

**Nếu KHÔNG có**: Chạy file `CreateAuctionTables.sql`

---

### 2. Kiểm tra có sản phẩm để tạo đấu giá không

**Điều kiện**: 
- Bạn phải có ít nhất 1 sản phẩm
- Sản phẩm phải `is_active = 1`
- Sản phẩm phải thuộc về bạn (seller_id = user hiện tại)

```sql
-- Kiểm tra sản phẩm của user
SELECT id, name, price, is_active, seller_id
FROM products
WHERE seller_id = <YOUR_USER_ID> AND is_active = 1;
```

**Nếu KHÔNG có sản phẩm**: 
1. Vào **"Sản phẩm của tôi"**
2. Click **"Đăng sản phẩm mới"**
3. Tạo sản phẩm trước

---

### 3. Kiểm tra lỗi validation

Khi submit form, các trường bắt buộc:

#### ✅ Giá khởi điểm (Starting Price)
- Phải > 0
- Ví dụ: `50000000` (50 triệu)

#### ✅ Thời gian bắt đầu (Start Time)
- Phải SAU thời điểm hiện tại
- Format: `dd/MM/yyyy HH:mm`
- Ví dụ: `15/11/2025 10:00`

#### ✅ Thời gian kết thúc (End Time)
- Phải SAU thời gian bắt đầu
- Ví dụ: `20/11/2025 18:00`

#### ⚠️ Giá dự trữ (Reserve Price) - Optional
- Nếu nhập, phải > Giá khởi điểm
- Có thể để trống

---

### 4. Kiểm tra lỗi trong browser

**Mở Developer Tools** (F12):
1. Tab **Console**: Xem có lỗi JavaScript không
2. Tab **Network**: Xem request POST có lỗi gì không

---

### 5. Test case mẫu

#### Dữ liệu hợp lệ:
```
Sản phẩm: [Chọn từ dropdown]
Giá khởi điểm: 50000000
Giá dự trữ: 70000000 (hoặc để trống)
Thời gian bắt đầu: 10/11/2025 14:00
Thời gian kết thúc: 15/11/2025 18:00
```

---

## Lỗi thường gặp

### Lỗi 1: "Sản phẩm không tồn tại"
**Nguyên nhân**: Chưa có sản phẩm hoặc sản phẩm không active
**Giải pháp**: Tạo sản phẩm mới trước

### Lỗi 2: "Thời gian bắt đầu phải sau thời điểm hiện tại"
**Nguyên nhân**: Chọn thời gian trong quá khứ
**Giải pháp**: Chọn thời gian tương lai

### Lỗi 3: "Thời gian kết thúc phải sau thời gian bắt đầu"
**Nguyên nhân**: End Time <= Start Time
**Giải pháp**: Đảm bảo End Time > Start Time

### Lỗi 4: "Giá dự trữ phải lớn hơn giá khởi điểm"
**Nguyên nhân**: Reserve Price <= Starting Price
**Giải pháp**: Reserve Price > Starting Price hoặc để trống

### Lỗi 5: "Invalid column name"
**Nguyên nhân**: Chưa chạy SQL script hoặc DbContext chưa cập nhật
**Giải pháp**: 
1. Chạy `CreateAuctionTables.sql`
2. Stop Visual Studio
3. Build lại: `dotnet build`

---

## Cách test nhanh

### Bước 1: Tạo sản phẩm test
1. Đăng nhập
2. Vào "Sản phẩm của tôi" → "Đăng sản phẩm mới"
3. Tạo 1 sản phẩm bất kỳ

### Bước 2: Tạo đấu giá
1. Vào "Đấu giá của tôi" → "Tạo đấu giá mới"
2. Chọn sản phẩm vừa tạo
3. Nhập:
   - Giá khởi điểm: `10000000`
   - Thời gian bắt đầu: Ngày mai
   - Thời gian kết thúc: 5 ngày sau
4. Click "Tạo đấu giá"

### Bước 3: Kiểm tra
- Vào "Đấu giá của tôi" → Phải thấy đấu giá vừa tạo
- Vào "Đấu giá" (public) → Phải thấy trong danh sách

---

## Nếu vẫn lỗi

Gửi cho tôi:
1. Screenshot lỗi
2. Hoặc copy message lỗi chính xác
3. Hoặc check Console log (F12)
