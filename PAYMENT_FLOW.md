# 💰 LUỒNG THANH TOÁN ĐẤU GIÁ

## 🎯 Cơ chế thanh toán

### Đấu giá thắng cuộc = Đã thanh toán 100%

Khi seller đóng đấu giá và chọn người thắng:

1. ✅ **Trừ tiền từ ví winner**
   - Số tiền: Giá đặt cao nhất (ví dụ: 8,000,000 VND)
   - Loại giao dịch: `payment`
   - Mô tả: "Thanh toán đấu giá #123"

2. ✅ **Cộng tiền vào ví seller**
   - Số tiền: 8,000,000 VND
   - Loại giao dịch: `refund`
   - Mô tả: "Thu tiền từ đấu giá #123"

3. ✅ **Tạo đơn hàng với giá 0đ**
   - Buyer: Winner
   - Seller: Seller
   - **Giá sản phẩm: 0 VND** ← Vì đã thanh toán qua ví
   - Note: "Đơn hàng từ đấu giá #123. Đã thanh toán 8,000,000 VND qua ví."

---

## 📊 So sánh: Đấu giá vs Mua thường

### Mua thường (qua giỏ hàng)
```
1. Thêm vào giỏ hàng
2. Checkout
3. Tạo đơn hàng với giá = giá sản phẩm
4. Chờ thanh toán
5. Seller giao hàng
```

### Đấu giá (qua ví)
```
1. Đặt giá (cần có tiền trong ví)
2. Seller chọn người thắng
3. ✅ Thanh toán TỰ ĐỘNG qua ví
4. ✅ Tạo đơn hàng với giá = 0đ
5. Seller giao hàng
```

---

## 💡 Tại sao giá = 0đ?

### Lý do:
- Winner **ĐÃ THANH TOÁN** qua ví khi seller chốt đấu giá
- Tiền đã chuyển từ ví winner → ví seller
- Đơn hàng chỉ để **QUẢN LÝ GIAO HÀNG**, không phải để thanh toán

### Lợi ích:
1. ✅ **Không thanh toán 2 lần**
   - Winner không phải trả thêm tiền
   - Tránh nhầm lẫn

2. ✅ **Rõ ràng trong lịch sử**
   - Ví: Có giao dịch thanh toán
   - Đơn hàng: Ghi chú đã thanh toán bao nhiêu

3. ✅ **Seller đã nhận tiền**
   - Không cần chờ buyer thanh toán
   - Chỉ cần giao hàng

---

## 📋 Ví dụ cụ thể

### Trước khi đấu giá
```
Ví Winner:  10,000,000 VND
Ví Seller:   5,000,000 VND
```

### Đấu giá
```
Winner đặt giá: 8,000,000 VND
(Hệ thống kiểm tra: 10M >= 8M ✅)
```

### Seller đóng đấu giá
```
1. Trừ ví Winner:  10,000,000 - 8,000,000 = 2,000,000 VND
2. Cộng ví Seller:  5,000,000 + 8,000,000 = 13,000,000 VND
3. Tạo đơn hàng:
   - Sản phẩm: Tesla Model 3
   - Giá: 0 VND
   - Note: "Đã thanh toán 8,000,000 VND qua ví"
```

### Sau khi đấu giá
```
Ví Winner:   2,000,000 VND ✅
Ví Seller:  13,000,000 VND ✅
Đơn hàng:    0 VND (đã thanh toán) ✅
```

---

## 🔍 Xem lịch sử thanh toán

### Winner xem ví
```
┌─────────────────────────────────────────┐
│ Lịch sử giao dịch                       │
├─────────────────────────────────────────┤
│ 10/11/2025 13:00                        │
│ [Thanh toán] -8,000,000 VND             │
│ Thanh toán đấu giá #123                 │
│ Số dư sau: 2,000,000 VND                │
└─────────────────────────────────────────┘
```

### Winner xem đơn hàng
```
┌─────────────────────────────────────────┐
│ Đơn hàng #456                           │
├─────────────────────────────────────────┤
│ Tesla Model 3                           │
│ Số lượng: 1                             │
│ Giá: 0 VND                              │
│                                         │
│ Ghi chú:                                │
│ "Đơn hàng từ đấu giá #123.              │
│  Đã thanh toán 8,000,000 VND qua ví."   │
│                                         │
│ Trạng thái: Chờ giao hàng               │
└─────────────────────────────────────────┘
```

### Seller xem ví
```
┌─────────────────────────────────────────┐
│ Lịch sử giao dịch                       │
├─────────────────────────────────────────┤
│ 10/11/2025 13:00                        │
│ [Hoàn tiền] +8,000,000 VND              │
│ Thu tiền từ đấu giá #123                │
│ Số dư sau: 13,000,000 VND               │
└─────────────────────────────────────────┘
```

---

## ⚠️ Lưu ý quan trọng

### 1. Đơn hàng giá 0đ là BÌNH THƯỜNG
- ✅ Không phải lỗi
- ✅ Đã thanh toán qua ví
- ✅ Chỉ để quản lý giao hàng

### 2. Winner không cần thanh toán thêm
- ✅ Đã trừ tiền khi seller chốt
- ✅ Chỉ cần đợi nhận hàng

### 3. Seller đã nhận tiền
- ✅ Tiền đã vào ví
- ✅ Có thể rút hoặc dùng tiếp

### 4. Không thể hoàn tác
- ❌ Sau khi chốt, không thể hủy
- ❌ Tiền đã chuyển không thể lấy lại
- ✅ Đảm bảo cam kết của cả 2 bên

---

## 🎯 Tổng kết

### Luồng thanh toán đấu giá:
1. Winner đặt giá (cần có tiền trong ví)
2. Seller chọn người thắng
3. **Thanh toán TỰ ĐỘNG**: Ví winner → Ví seller
4. **Tạo đơn hàng giá 0đ** (đã thanh toán)
5. Seller giao hàng
6. Winner nhận hàng
7. Hoàn tất

### Ưu điểm:
- ✅ Thanh toán tức thì
- ✅ Không cần bước thanh toán thủ công
- ✅ Seller nhận tiền ngay
- ✅ Minh bạch, rõ ràng
- ✅ Không thể gian lận

### Bảo mật:
- ✅ Kiểm tra số dư trước khi chốt
- ✅ Giao dịch atomic (hoặc thành công hoặc thất bại)
- ✅ Lưu lịch sử đầy đủ
- ✅ Không thể hoàn tác sau khi chốt
