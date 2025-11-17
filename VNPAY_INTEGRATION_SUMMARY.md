# ✅ TÍCH HỢP VNPAY PAYMENT GATEWAY

## 🎯 Đã hoàn thành

### 1. ✅ VNPayService & Helper Classes
**File:** `BLL/Services/VNPayService.cs`

**Tính năng:**
- ✅ `IVNPayService` interface
- ✅ `VNPayService` implementation
- ✅ `VNPayLibrary` - Helper class xử lý request/response
- ✅ `VNPayCompare` - Comparer cho sorting parameters
- ✅ HMAC SHA512 signature generation
- ✅ URL encoding và query string building
- ✅ Signature validation

**Methods:**
```csharp
string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, string returnUrl)
bool ValidateSignature(Dictionary<string, string> queryParams, string secureHash)
```

---

### 2. ✅ Cấu hình VNPay
**File:** `appsettings.json`

```json
"VNPay": {
  "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
  "TmnCode": "TSFZ2A2L",
  "HashSecret": "SNIHOORBOFJ6USCIPO48W9H6NYPBAKI4",
  "ReturnUrl": "https://localhost:5001/Payment/VNPayCallback",
  "Version": "2.1.0",
  "Command": "pay"
}
```

**Đăng ký service:** `Program.cs`
```csharp
builder.Services.AddScoped<IVNPayService, VNPayService>();
```

---

### 3. ✅ Nạp ví qua VNPay
**Files:**
- `Pages/Wallet/Deposit.cshtml`
- `Pages/Wallet/Deposit.cshtml.cs`

**Tính năng:**
- ✅ Chọn phương thức: VNPay (khuyến nghị) hoặc Demo
- ✅ Nút "Thanh toán qua VNPay" → Redirect sang VNPay
- ✅ Nút "Nạp tiền Demo" → Nạp trực tiếp (test)
- ✅ JavaScript toggle nút dựa trên lựa chọn
- ✅ Transaction ID format: `WALLET_{userId}_{timestamp}`

**Luồng:**
```
User nhập số tiền → Chọn VNPay
    ↓
Tạo payment URL với VNPayService
    ↓
Redirect sang VNPay sandbox
    ↓
User thanh toán trên VNPay
    ↓
VNPay callback về /Payment/VNPayCallback
    ↓
Validate signature → Cập nhật số dư ví
```

---

### 4. ✅ Thanh toán đơn hàng qua VNPay
**Files:**
- `Pages/Cart/Checkout.cshtml`
- `Pages/Cart/Checkout.cshtml.cs`

**Tính năng:**
- ✅ 3 phương thức thanh toán:
  - **VNPay** (online, khuyến nghị)
  - **COD** (thanh toán khi nhận hàng)
  - **Chuyển khoản** (ngân hàng)
- ✅ Nút "Thanh toán qua VNPay" → Redirect VNPay
- ✅ Nút "Đặt hàng" → Tạo đơn COD/Bank
- ✅ Lưu thông tin đơn hàng vào session
- ✅ Transaction ID format: `ORDER_{userId}_{timestamp}`

**Handlers:**
```csharp
OnPostVNPayAsync() // Thanh toán VNPay
OnPostPlaceOrderAsync() // Đặt hàng COD/Bank
```

---

### 5. ✅ VNPay Callback Handler
**Files:**
- `Pages/Payment/VNPayCallback.cshtml`
- `Pages/Payment/VNPayCallback.cshtml.cs`

**Tính năng:**
- ✅ Nhận callback từ VNPay
- ✅ Validate signature (bảo mật)
- ✅ Parse response parameters
- ✅ Xử lý 2 loại giao dịch:
  - **Nạp ví:** `WALLET_*` → Cập nhật số dư
  - **Đơn hàng:** `ORDER_*` → Tạo đơn hàng
- ✅ UI hiển thị kết quả:
  - ✅ Thành công → Icon xanh, thông tin GD
  - ✅ Thất bại → Icon đỏ, lý do lỗi
- ✅ Response code mapping (24 mã lỗi VNPay)

**Response Codes:**
```
00 - Thành công
07 - Nghi ngờ gian lận
09 - Chưa đăng ký InternetBanking
11 - Hết hạn chờ thanh toán
12 - Thẻ bị khóa
24 - Khách hàng hủy
51 - Không đủ số dư
75 - Ngân hàng bảo trì
...
```

---

## 🎨 Giao diện

### Nạp ví
```
┌─────────────────────────────────────┐
│ Nạp tiền vào ví                     │
├─────────────────────────────────────┤
│ Số dư: 1,000,000 VND                │
│                                     │
│ Số tiền: [_________]                │
│ [100K] [500K] [1M] [5M]             │
│                                     │
│ Phương thức:                        │
│ ☑ VNPay [Khuyến nghị]               │
│ ☐ Demo (Test)                       │
│                                     │
│ [Thanh toán qua VNPay]              │
└─────────────────────────────────────┘
```

### Checkout
```
┌─────────────────────────────────────┐
│ Thanh toán                          │
├─────────────────────────────────────┤
│ Địa chỉ: [________________]         │
│                                     │
│ Phương thức:                        │
│ ☑ VNPay [Thanh toán online]         │
│ ☐ COD (Khi nhận hàng)               │
│ ☐ Chuyển khoản                      │
│                                     │
│ [Thanh toán qua VNPay]              │
│                                     │
│ Tổng: 5,000,000 VND                 │
└─────────────────────────────────────┘
```

### Callback Success
```
┌─────────────────────────────────────┐
│ ✓ Thanh toán thành công!            │
├─────────────────────────────────────┤
│         [Icon xanh lớn]             │
│                                     │
│ Mã GD: WALLET_1_20251110...         │
│ Số tiền: 1,000,000 VND              │
│ Nội dung: Nạp tiền ví               │
│ Thời gian: 10/11/2025 22:30         │
│                                     │
│ [Xem ví của tôi]                    │
└─────────────────────────────────────┘
```

---

## 📋 Luồng hoạt động

### Luồng nạp ví
```
1. User vào Wallet → Deposit
2. Nhập số tiền (min 10,000 VND)
3. Chọn VNPay
4. Click "Thanh toán qua VNPay"
   ↓
5. VNPayService.CreatePaymentUrl()
   - Transaction ID: WALLET_1_20251110223045
   - Amount: 1000000 * 100 = 100000000
   - OrderInfo: "Nap tien vi - John Doe"
   - Signature: HMAC SHA512
   ↓
6. Redirect sang VNPay sandbox
7. User đăng nhập & thanh toán
   ↓
8. VNPay callback: /Payment/VNPayCallback
9. Validate signature
10. Parse response
11. Check transaction ID starts with "WALLET_"
12. WalletService.AddBalanceAsync()
13. Hiển thị kết quả
```

### Luồng thanh toán đơn hàng
```
1. User vào Cart → Checkout
2. Nhập địa chỉ giao hàng
3. Chọn VNPay
4. Click "Thanh toán qua VNPay"
   ↓
5. Lưu CreateOrderDto vào session
6. VNPayService.CreatePaymentUrl()
   - Transaction ID: ORDER_1_20251110223045
   - Amount: 5000000 * 100
   - OrderInfo: "Thanh toan don hang - John Doe"
   ↓
7. Redirect VNPay
8. User thanh toán
   ↓
9. VNPay callback
10. Validate signature
11. Check transaction ID starts with "ORDER_"
12. Lấy CreateOrderDto từ session
13. OrderService.CreateOrderFromCartAsync()
14. Clear cart & session
15. Hiển thị kết quả
```

---

## 🔧 Cách test

### Test nạp ví VNPay

**Bước 1:** Dừng Visual Studio
```
Shift + F5
Đóng VS
```

**Bước 2:** Chạy lại
```bash
cd "c:\Users\Hp\Desktop\Bai Thi Pe\PRN222_FinalProject"
dotnet run --project PRN222_FinalProject
```

**Bước 3:** Test
```
1. Login
2. Vào "Ví của tôi" → "Nạp tiền"
3. Nhập: 100,000 VND
4. Chọn "VNPay"
5. Click "Thanh toán qua VNPay"
6. Sẽ redirect sang VNPay sandbox
7. Dùng thẻ test VNPay để thanh toán
8. Sau khi thanh toán → Callback về
9. Kiểm tra số dư đã tăng
```

### Test thanh toán đơn hàng

```
1. Thêm sản phẩm vào giỏ
2. Vào "Giỏ hàng" → "Thanh toán"
3. Nhập địa chỉ
4. Chọn "VNPay"
5. Click "Thanh toán qua VNPay"
6. Thanh toán trên VNPay
7. Callback về → Đơn hàng được tạo
8. Kiểm tra "Đơn hàng" → Có đơn mới
```

### Thẻ test VNPay Sandbox

```
Ngân hàng: NCB
Số thẻ: 9704198526191432198
Tên chủ thẻ: NGUYEN VAN A
Ngày phát hành: 07/15
Mật khẩu OTP: 123456
```

---

## ✅ Kết quả

### Trước:
- ❌ Chỉ có nạp ví demo
- ❌ Không có thanh toán online
- ❌ Không tích hợp payment gateway

### Sau:
- ✅ Nạp ví qua VNPay
- ✅ Thanh toán đơn hàng qua VNPay
- ✅ Callback handler xử lý response
- ✅ Signature validation (bảo mật)
- ✅ UI đẹp, UX tốt
- ✅ Hỗ trợ cả COD và chuyển khoản

---

## 📦 Files đã tạo/sửa

### Tạo mới:
1. `BLL/Services/VNPayService.cs` - VNPay service & helpers
2. `Pages/Payment/VNPayCallback.cshtml` - Callback UI
3. `Pages/Payment/VNPayCallback.cshtml.cs` - Callback logic

### Cập nhật:
1. `appsettings.json` - Thêm VNPay config
2. `Program.cs` - Đăng ký VNPayService
3. `Pages/Wallet/Deposit.cshtml` - Thêm VNPay option
4. `Pages/Wallet/Deposit.cshtml.cs` - Handler VNPay
5. `Pages/Cart/Checkout.cshtml` - Thêm VNPay option
6. `Pages/Cart/Checkout.cshtml.cs` - Handler VNPay

---

## 🎉 HOÀN THÀNH 100%

**Đã tích hợp VNPay cho:**
1. ✅ Nạp ví
2. ✅ Thanh toán đơn hàng
3. ✅ Callback handler
4. ✅ Signature validation
5. ✅ UI/UX hoàn chỉnh

**Chỉ cần dừng VS và chạy lại để test!**
