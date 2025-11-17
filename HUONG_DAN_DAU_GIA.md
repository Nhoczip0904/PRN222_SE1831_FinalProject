# Hướng dẫn luồng đấu giá và thanh toán

## 🎯 Luồng hoàn chỉnh từ đấu giá đến nhận tiền

### 1️⃣ Người bán tạo phiên đấu giá
- Tạo sản phẩm đấu giá với giá khởi điểm
- Đặt thời gian bắt đầu và kết thúc
- Người mua bắt đầu đặt giá

### 2️⃣ Người bán chốt người thắng đấu giá
**Khi người bán bấm "Chốt đấu giá":**

#### ✅ Hệ thống tự động thực hiện:
1. **Kiểm tra số dư ví người thắng**
   - Phải đủ tiền để thanh toán giá đấu giá

2. **Trừ tiền từ ví người thắng**
   - Số tiền = Giá đấu giá cao nhất
   - Ghi chú: "Thanh toán đấu giá #X - Chờ xác nhận giao hàng"
   - Tiền được giữ trong hệ thống

3. **Tạo đơn hàng tự động**
   - Đơn hàng được tạo với giá = Giá đấu giá
   - Payment method = "auction"
   - Badge màu vàng: 🔨 Đấu giá

4. **Tự động xác nhận đơn từ phía người bán**
   - Status chuyển từ `pending` → `confirmed`
   - Bỏ qua bước chờ người bán xác nhận

### 3️⃣ Người bán bàn giao xe
- Vào **Đơn hàng của tôi** → Tab **Đơn bán**
- Bấm nút **"Bàn giao xe"**
- Status chuyển thành: `shipped` (Đang giao)

### 4️⃣ Người mua xác nhận đã nhận hàng
**Đây là bước quan trọng để chia tiền!**

#### Cách thực hiện:
1. Vào **Đơn hàng của tôi** → Tab **Đơn mua**
2. Tìm đơn hàng có status **"Đang giao"**
3. Bấm nút **"✓ Đã nhận hàng"**
4. Xác nhận trong popup

#### ✅ Hệ thống tự động chia tiền:
- **75%** → Cộng vào ví người bán
- **25%** → Giữ lại làm phí hoa hồng hệ thống
- Ghi nhận vào bảng `system_revenues`

### 5️⃣ Gửi admin duyệt hợp đồng (nếu cần)
- Admin có thể xem tất cả đơn hàng tại `/Admin/Orders`
- Duyệt hợp đồng tại `/Admin/Contracts`

---

## 💰 Ví dụ cụ thể

### Phiên đấu giá:
- **Giá khởi điểm:** 100,000 VND
- **Giá thắng:** 1,000,000 VND
- **Người thắng:** Nguyễn Văn A

### Khi người bán chốt đấu giá:
```
Ví người thắng (Nguyễn Văn A):
- Trước: 5,000,000 VND
- Sau:  4,000,000 VND (-1,000,000 VND)
- Ghi chú: "Thanh toán đấu giá #123 - Chờ xác nhận giao hàng"

Đơn hàng được tạo:
- ID: #456
- Tổng tiền: 1,000,000 VND
- Status: confirmed (tự động)
- Payment method: auction
- Badge: 🔨 Đấu giá
```

### Khi người mua xác nhận đã nhận hàng:
```
Ví người bán (Trần Thị B):
+ 750,000 VND (75%)
Ghi chú: "Thanh toán từ đơn hàng #456"

System Revenue:
+ 250,000 VND (25%)
Ghi chú: "Phí hoa hồng 25% từ đơn hàng #456"
```

---

## 📊 Bảng biến động số dư

### ✅ Người mua (Nguyễn Văn A):
| Thời gian | Loại | Số tiền | Số dư sau | Mô tả |
|-----------|------|---------|-----------|-------|
| 11/11 10:43 | Trừ | -1,000,000 | 4,000,000 | Thanh toán đấu giá #123 - Chờ xác nhận giao hàng |

### ✅ Người bán (Trần Thị B):
| Thời gian | Loại | Số tiền | Số dư sau | Mô tả |
|-----------|------|---------|-----------|-------|
| 11/11 15:30 | Cộng | +750,000 | 2,750,000 | Thanh toán từ đơn hàng #456 |

**Lưu ý:** Người bán chỉ thấy số tiền nhận được (75%), không hiển thị bị trừ 25%

---

## 🔄 So sánh với đơn hàng thường

### Đơn hàng thường:
1. Người mua đặt hàng → Trừ tiền ngay
2. Người bán xác nhận → Status: confirmed
3. Người bán giao hàng → Status: shipped
4. Người mua xác nhận → Chia 75/25

### Đơn hàng từ đấu giá:
1. Người bán chốt đấu giá → Trừ tiền ngay + Tạo đơn + **Tự động confirmed**
2. Người bán giao hàng → Status: shipped
3. Người mua xác nhận → Chia 75/25

**Khác biệt:** Đơn từ đấu giá bỏ qua bước "pending", tự động chuyển sang "confirmed"

---

## ⚠️ Lưu ý quan trọng

### Cho người bán:
- ✅ Chỉ chốt người thắng khi chắc chắn có thể giao hàng
- ✅ Tiền sẽ được trừ từ ví người thắng ngay lập tức
- ✅ Đơn hàng tự động được xác nhận, chỉ cần bàn giao xe
- ⚠️ Nếu không giao được hàng, cần liên hệ admin để hoàn tiền

### Cho người mua:
- ✅ Đảm bảo có đủ tiền trong ví trước khi đấu giá
- ✅ Tiền sẽ bị trừ ngay khi người bán chốt đấu giá
- ✅ Kiểm tra kỹ hàng trước khi xác nhận đã nhận
- ⚠️ Sau khi xác nhận, tiền sẽ được chuyển cho người bán (không hoàn lại)

### Cho admin:
- ✅ Có thể xem tất cả đơn hàng từ đấu giá (có badge 🔨)
- ✅ Doanh thu 25% được ghi nhận tự động
- ✅ Xem tổng commission tại Dashboard

---

## 🎨 Giao diện

### Trang đơn hàng của người mua:
```
┌─────────────────────────────────────────────────────┐
│ Đơn hàng #456  11/11/2025 10:43  [🔨 Đấu giá]      │
│                                    [Đã xác nhận]    │
├─────────────────────────────────────────────────────┤
│ 🏪 Người bán: Trần Thị B                            │
│ 📍 Địa chỉ: 123 Đường ABC                           │
│                                                      │
│ Sản phẩm:                                           │
│ Tesla 3990 x 1                    1,000,000 VND     │
│                                                      │
│ Tổng: 1,000,000 VND                                 │
│                                                      │
│ [Hợp đồng PDF]                                      │
└─────────────────────────────────────────────────────┘
```

### Khi status = shipped:
```
│ [Hợp đồng PDF] [✓ Đã nhận hàng]                    │
```

### Sau khi xác nhận:
```
│ [Hợp đồng PDF] [✓ Đã hoàn thành]                   │
```

---

## 📈 Theo dõi doanh thu (Admin)

### Dashboard Admin:
- **Card "Tổng đơn hàng"**: Tổng số đơn (bao gồm cả đấu giá)
- **Card "Tổng doanh thu"**: Tổng tiền tất cả đơn hàng
- **Card "Phí hoa hồng (25%)"**: Tổng commission từ tất cả đơn

### Chi tiết:
- Vào `/Admin/Orders` để xem danh sách đơn hàng
- Đơn từ đấu giá có badge 🔨 màu vàng
- Filter theo status để xem đơn đã hoàn thành
