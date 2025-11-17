# Hướng dẫn xác nhận giao hàng

## 📦 Luồng đơn hàng hoàn chỉnh

### 1. Người mua đặt hàng
- Vào trang **Giỏ hàng** → Bấm **Thanh toán**
- Chọn phương thức thanh toán:
  - **Ví của tôi**: Trừ tiền ngay từ ví
  - **VNPay**: Thanh toán online qua VNPay
- Đơn hàng được tạo với trạng thái: **Chờ xử lý** (pending)

### 2. Người bán xác nhận
- Vào **Đơn hàng của tôi** → Tab **Đơn bán**
- Xem đơn hàng mới → Xác nhận đơn
- Trạng thái chuyển thành: **Đã xác nhận** (confirmed)

### 3. Admin duyệt hợp đồng (nếu có)
- Admin vào `/Admin/Contracts`
- Duyệt hợp đồng của đơn hàng
- Trạng thái hợp đồng: **Đã duyệt** (approved)

### 4. Người bán bàn giao xe
- Sau khi hợp đồng được duyệt
- Vào **Đơn hàng của tôi** → Tab **Đơn bán**
- Bấm nút **"Bàn giao xe"** (màu xanh dương)
- Trạng thái chuyển thành: **Đang giao** (shipped)

### 5. 🎯 Người mua xác nhận đã nhận hàng
**Đây là bước quan trọng nhất!**

#### Cách thực hiện:
1. Vào trang **Đơn hàng của tôi** (`/Orders/Index`)
2. Chọn tab **Đơn mua**
3. Tìm đơn hàng có trạng thái **"Đang giao"** (shipped)
4. Bấm nút **"Đã nhận hàng"** (màu xanh lá)
5. Xác nhận trong popup: "Xác nhận đã nhận hàng? Tiền sẽ được chuyển cho người bán."

#### Điều gì xảy ra khi xác nhận:
- ✅ Trạng thái đơn hàng chuyển thành: **Đã giao** (delivered)
- ✅ Hệ thống tự động:
  - Cộng **75%** giá trị đơn hàng vào ví người bán
  - Giữ lại **25%** làm phí hoa hồng cho hệ thống
- ✅ Người bán nhận được thông báo và có thể rút tiền từ ví

## 💰 Ví dụ cụ thể

**Đơn hàng trị giá: 10,000,000 VND**

### Khi đặt hàng:
- Người mua trả: 10,000,000 VND (qua ví hoặc VNPay)
- Tiền được giữ trong hệ thống

### Khi xác nhận đã nhận hàng:
- Người bán nhận: **7,500,000 VND** (75%)
- Hệ thống giữ lại: **2,500,000 VND** (25% phí)

## 📊 Xem doanh thu (Admin)

Admin có thể xem tổng doanh thu tại:
- **Dashboard Admin** (`/Admin/Dashboard`)
  - Card "Phí hoa hồng (25%)" hiển thị tổng tiền hệ thống đã kiếm được
- **Quản lý đơn hàng** (`/Admin/Orders`)
  - Xem tất cả đơn hàng và trạng thái

## ⚠️ Lưu ý quan trọng

1. **Chỉ người mua mới có thể xác nhận đã nhận hàng**
2. **Chỉ có thể xác nhận khi trạng thái là "Đang giao" (shipped)**
3. **Sau khi xác nhận, không thể hoàn tác**
4. **Tiền sẽ được chuyển cho người bán ngay lập tức**
5. **Người mua nên kiểm tra kỹ hàng trước khi xác nhận**

## 🔄 Các trạng thái đơn hàng

1. **pending** - Chờ xử lý
2. **confirmed** - Đã xác nhận
3. **shipped** - Đang giao ← **Người mua bấm "Đã nhận hàng" ở đây**
4. **delivered** - Đã giao ← **Trạng thái cuối cùng, tiền được chia**
5. **cancelled** - Đã hủy

## 📱 Giao diện

### Tab "Đơn mua" của người mua:
```
┌─────────────────────────────────────────────┐
│ Đơn hàng #123                    [Đang giao]│
├─────────────────────────────────────────────┤
│ Người bán: Nguyễn Văn A                     │
│ Địa chỉ: 123 Đường ABC                      │
│ Sản phẩm: Pin xe điện x 1                   │
│ Tổng: 10,000,000 VND                        │
│                                              │
│ [Hợp đồng PDF] [✓ Đã nhận hàng]            │
└─────────────────────────────────────────────┘
```

Khi bấm nút **"Đã nhận hàng"**:
- Popup xác nhận xuất hiện
- Sau khi xác nhận → Trạng thái chuyển thành "Đã hoàn thành"
- Nút biến thành badge xanh: **"✓ Đã hoàn thành"**
