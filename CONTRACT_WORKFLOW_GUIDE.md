# Hướng dẫn Quy trình Hợp đồng Số

## Tổng quan

Hệ thống đã được tích hợp tính năng **Hợp đồng Số** với quy trình xác nhận 2 bên và phê duyệt của admin trước khi bàn giao xe.

## Quy trình Hoàn chỉnh

### 1. Tạo Hợp đồng (Tự động)

Khi người mua đặt hàng thành công, hệ thống tự động tạo hợp đồng số:

```
Đặt hàng → Tạo Order → Tự động tạo Contract
```

**Trạng thái ban đầu:** `pending` (Chờ xác nhận)

**File liên quan:**
- `BLL/Services/OrderService.cs` - Line 120-129
- `BLL/Services/ContractService.cs` - Method `CreateContractFromOrderAsync`

### 2. Xác nhận của Người Mua (Buyer)

**Bước thực hiện:**
1. Người mua vào trang **"Hợp đồng của tôi"** (`/Contracts/Index`)
2. Chọn hợp đồng cần xác nhận
3. Click **"Xem chi tiết & Xác nhận"**
4. Đọc kỹ nội dung hợp đồng
5. Click nút **"Tôi xác nhận hợp đồng (Người mua)"**

**Kết quả:**
- `BuyerConfirmed` = true
- `BuyerConfirmedAt` = thời gian xác nhận
- Lưu log xác nhận vào bảng `contract_confirmations`

### 3. Xác nhận của Người Bán (Seller)

**Bước thực hiện:**
1. Người bán vào trang **"Hợp đồng của tôi"** (`/Contracts/Index`)
2. Chọn hợp đồng cần xác nhận
3. Click **"Xem chi tiết & Xác nhận"**
4. Đọc kỹ nội dung hợp đồng
5. Click nút **"Tôi xác nhận hợp đồng (Người bán)"**

**Kết quả:**
- `SellerConfirmed` = true
- `SellerConfirmedAt` = thời gian xác nhận
- Lưu log xác nhận vào bảng `contract_confirmations`

**Quan trọng:** Khi CẢ 2 BÊN đã xác nhận:
- Trạng thái hợp đồng tự động chuyển sang `confirmed`
- Hợp đồng xuất hiện trong danh sách chờ duyệt của Admin

### 4. Phê duyệt của Admin

**Bước thực hiện:**
1. Admin đăng nhập vào trang quản trị
2. Vào **"Duyệt hợp đồng"** (`/Admin/Contracts/Index`)
3. Xem danh sách hợp đồng đã được 2 bên xác nhận
4. Kiểm tra thông tin hợp đồng
5. Chọn một trong hai:
   - **Duyệt:** Click "Duyệt hợp đồng"
   - **Từ chối:** Click "Từ chối" và nhập lý do

**Kết quả khi DUYỆT:**
- `AdminApproved` = true
- `AdminApprovedAt` = thời gian duyệt
- `AdminApprovedBy` = ID của admin
- Trạng thái hợp đồng: `approved`
- **Trạng thái đơn hàng tự động chuyển sang `confirmed`** (Sẵn sàng bàn giao)
- Lưu log phê duyệt

**Kết quả khi TỪ CHỐI:**
- Trạng thái hợp đồng: `rejected`
- `RejectionReason` = lý do từ chối
- Lưu log từ chối
- Người mua và người bán có thể xem lý do từ chối

### 5. Bàn giao Xe (Chỉ sau khi hợp đồng được duyệt)

**Điều kiện bắt buộc:**
- Hợp đồng phải có trạng thái `approved`
- Admin đã phê duyệt (`AdminApproved` = true)

**Bước thực hiện:**
1. Người bán vào **"Đơn bán"** (`/Orders/Index?viewType=seller`)
2. Tìm đơn hàng có trạng thái `confirmed`
3. Nếu hợp đồng đã được duyệt → Hiện nút **"Bàn giao xe"**
4. Nếu hợp đồng chưa được duyệt → Hiện nút disabled **"Chờ duyệt hợp đồng"**
5. Click **"Bàn giao xe"** để cập nhật trạng thái đơn hàng sang `shipped`

**File liên quan:**
- `Pages/Orders/Index.cshtml` - Line 109-126

## Sơ đồ Trạng thái Hợp đồng

```
pending (Chờ xác nhận)
    ↓
    ├─→ Buyer xác nhận → BuyerConfirmed = true
    └─→ Seller xác nhận → SellerConfirmed = true
    ↓
confirmed (Cả 2 bên đã xác nhận - Chờ admin)
    ↓
    ├─→ Admin duyệt → approved (Đã duyệt) → Cho phép bàn giao xe
    └─→ Admin từ chối → rejected (Đã từ chối)
```

## Các Trang Chính

### 1. Danh sách Hợp đồng của User
**URL:** `/Contracts/Index`
**Chức năng:**
- Xem tất cả hợp đồng (mua & bán)
- Hiển thị trạng thái xác nhận của từng bên
- Link đến trang chi tiết để xác nhận

### 2. Chi tiết Hợp đồng
**URL:** `/Contracts/Details/{id}`
**Chức năng:**
- Xem thông tin chi tiết hợp đồng
- Xác nhận hợp đồng (nếu là buyer/seller và chưa xác nhận)
- Xem lịch sử xác nhận
- Xem lý do từ chối (nếu có)

### 3. Hợp đồng PDF
**URL:** `/Orders/Contract/{orderId}`
**Chức năng:**
- Xem hợp đồng dạng PDF
- In hợp đồng
- Tải về PDF

### 4. Duyệt Hợp đồng (Admin)
**URL:** `/Admin/Contracts/Index`
**Chức năng:**
- Xem danh sách hợp đồng chờ duyệt
- Duyệt hoặc từ chối hợp đồng
- Nhập lý do từ chối

### 5. Danh sách Đơn hàng
**URL:** `/Orders/Index`
**Chức năng:**
- Hiển thị trạng thái hợp đồng cho mỗi đơn hàng
- Link đến trang hợp đồng
- Chỉ cho phép bàn giao xe khi hợp đồng đã được duyệt

## Cấu trúc Database

### Bảng `contracts`
```sql
- id: ID hợp đồng
- order_id: ID đơn hàng
- buyer_id: ID người mua
- seller_id: ID người bán
- contract_number: Số hợp đồng (HD + ngày + order_id)
- total_amount: Tổng giá trị

-- Xác nhận
- buyer_confirmed: Người mua đã xác nhận?
- buyer_confirmed_at: Thời gian xác nhận
- seller_confirmed: Người bán đã xác nhận?
- seller_confirmed_at: Thời gian xác nhận

-- Phê duyệt
- admin_approved: Admin đã duyệt?
- admin_approved_at: Thời gian duyệt
- admin_approved_by: ID admin duyệt

-- Trạng thái
- status: pending/confirmed/approved/rejected/cancelled
- rejection_reason: Lý do từ chối
```

### Bảng `contract_confirmations`
```sql
- id: ID log
- contract_id: ID hợp đồng
- user_id: ID người thực hiện
- user_role: buyer/seller/admin
- action: confirmed/rejected/approved
- note: Ghi chú
- ip_address: IP address
- created_at: Thời gian
```

## API/Service Methods

### ContractService

```csharp
// Tạo hợp đồng từ order
CreateContractFromOrderAsync(int orderId)

// Lấy thông tin hợp đồng
GetContractByIdAsync(int contractId)
GetContractByOrderIdAsync(int orderId)

// Xác nhận
BuyerConfirmAsync(int contractId, int userId, string? ipAddress)
SellerConfirmAsync(int contractId, int userId, string? ipAddress)

// Phê duyệt
AdminApproveAsync(int contractId, int adminId, string? ipAddress)
AdminRejectAsync(int contractId, int adminId, string reason, string? ipAddress)

// Truy vấn
GetPendingContractsAsync() // Hợp đồng chờ admin duyệt
GetUserContractsAsync(int userId) // Hợp đồng của user
IsContractApprovedAsync(int orderId) // Kiểm tra đã duyệt chưa
```

## Luồng Nghiệp vụ Hoàn chỉnh

1. **Người mua đặt hàng**
   - Hệ thống tạo Order
   - Tự động tạo Contract với status = "pending"

2. **Người mua xác nhận hợp đồng**
   - Vào /Contracts/Index
   - Click vào hợp đồng → /Contracts/Details/{id}
   - Click "Tôi xác nhận hợp đồng (Người mua)"
   - BuyerConfirmed = true

3. **Người bán xác nhận hợp đồng**
   - Vào /Contracts/Index
   - Click vào hợp đồng → /Contracts/Details/{id}
   - Click "Tôi xác nhận hợp đồng (Người bán)"
   - SellerConfirmed = true
   - **Status tự động chuyển sang "confirmed"**

4. **Admin duyệt hợp đồng**
   - Vào /Admin/Contracts/Index
   - Thấy hợp đồng trong danh sách chờ duyệt
   - Click "Duyệt hợp đồng"
   - AdminApproved = true
   - Status = "approved"
   - **Order status tự động chuyển sang "confirmed"**

5. **Người bán bàn giao xe**
   - Vào /Orders/Index?viewType=seller
   - Thấy đơn hàng có status = "confirmed"
   - Kiểm tra hợp đồng đã approved
   - Click "Bàn giao xe"
   - Order status chuyển sang "shipped"

## Lưu ý Quan trọng

1. **Bắt buộc xác nhận 2 bên:** Hợp đồng chỉ được gửi admin duyệt khi CẢ 2 BÊN đã xác nhận

2. **Bắt buộc admin duyệt:** Không thể bàn giao xe nếu admin chưa duyệt hợp đồng

3. **Lưu lịch sử:** Mọi hành động xác nhận/duyệt đều được lưu log vào `contract_confirmations`

4. **IP Tracking:** Hệ thống lưu IP address của người thực hiện để audit

5. **Tự động tạo:** Hợp đồng được tạo tự động khi đặt hàng, không cần thao tác thủ công

6. **Số hợp đồng:** Format: HD + YYYYMMDD + OrderID (6 chữ số)
   - Ví dụ: HD20251111000001

## Testing Workflow

### Test Case 1: Quy trình thành công
1. User A (buyer) đặt hàng từ User B (seller)
2. Kiểm tra hợp đồng được tạo tự động
3. User A xác nhận hợp đồng
4. User B xác nhận hợp đồng
5. Kiểm tra status chuyển sang "confirmed"
6. Admin duyệt hợp đồng
7. Kiểm tra status chuyển sang "approved"
8. Kiểm tra order status chuyển sang "confirmed"
9. User B bàn giao xe (update order status sang "shipped")

### Test Case 2: Admin từ chối
1. Thực hiện bước 1-5 như Test Case 1
2. Admin từ chối hợp đồng với lý do
3. Kiểm tra status = "rejected"
4. Kiểm tra User A và B có thể xem lý do từ chối
5. Kiểm tra không thể bàn giao xe

### Test Case 3: Chỉ 1 bên xác nhận
1. User A đặt hàng
2. User A xác nhận hợp đồng
3. Kiểm tra status vẫn là "pending"
4. Kiểm tra hợp đồng KHÔNG xuất hiện trong danh sách admin
5. User B xác nhận
6. Kiểm tra status chuyển sang "confirmed"
7. Kiểm tra hợp đồng xuất hiện trong danh sách admin

## Files Modified/Created

### Created:
- `PRN222_FinalProject/Pages/Contracts/Index.cshtml`
- `PRN222_FinalProject/Pages/Contracts/Index.cshtml.cs`
- `CONTRACT_WORKFLOW_GUIDE.md`

### Modified:
- `BLL/Services/ContractService.cs`
  - Added `CreateContractFromOrderAsync`
  - Added `IsContractApprovedAsync`
  - Updated `AdminApproveAsync` to update order status

- `PRN222_FinalProject/Pages/Orders/Index.cshtml`
  - Added contract status display
  - Added contract approval check before vehicle handover
  - Changed "Đã giao" button to "Bàn giao xe"

- `PRN222_FinalProject/Pages/Orders/Index.cshtml.cs`
  - Added `IContractService` dependency
  - Added `OrderContracts` dictionary
  - Load contract info for each order

### Existing (Already implemented):
- `DAL/Entities/Contract.cs`
- `BLL/Services/ContractService.cs`
- `PRN222_FinalProject/Pages/Contracts/Details.cshtml`
- `PRN222_FinalProject/Pages/Contracts/Details.cshtml.cs`
- `PRN222_FinalProject/Pages/Admin/Contracts/Index.cshtml`
- `PRN222_FinalProject/Pages/Admin/Contracts/Index.cshtml.cs`
- `PRN222_FinalProject/Pages/Orders/Contract.cshtml`
- `CREATE_CONTRACTS_TABLE.sql`

## Kết luận

Hệ thống hợp đồng số đã được tích hợp hoàn chỉnh với:
- ✅ Tự động tạo hợp đồng khi đặt hàng
- ✅ Xác nhận 2 bên (buyer & seller)
- ✅ Phê duyệt của admin
- ✅ Chặn bàn giao xe nếu chưa có hợp đồng được duyệt
- ✅ Lưu lịch sử đầy đủ
- ✅ UI/UX thân thiện
- ✅ Tracking IP address
