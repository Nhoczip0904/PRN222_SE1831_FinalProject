# Hướng dẫn Cài đặt Tính năng Hợp đồng Số

## Bước 1: Cập nhật Database

Chạy script SQL để tạo bảng contracts và contract_confirmations:

```bash
# Mở SQL Server Management Studio hoặc Azure Data Studio
# Kết nối đến database của bạn
# Chạy file: CREATE_CONTRACTS_TABLE.sql
```

Hoặc chạy trực tiếp:

```sql
-- File: CREATE_CONTRACTS_TABLE.sql
-- Tạo bảng contracts và contract_confirmations
```

## Bước 2: Kiểm tra DbContext

Đảm bảo `EvBatteryTrading2Context` đã có các DbSet:

```csharp
public virtual DbSet<Contract> Contracts { get; set; }
public virtual DbSet<ContractConfirmation> ContractConfirmations { get; set; }
```

✅ **Đã có sẵn** - Không cần thay đổi

## Bước 3: Kiểm tra Service Registration

Đảm bảo `ContractService` đã được đăng ký trong `Program.cs`:

```csharp
builder.Services.AddScoped<IContractService, ContractService>();
```

✅ **Đã có sẵn** - Không cần thay đổi

## Bước 4: Build và Run

```bash
# Build project
dotnet build

# Run project
dotnet run --project PRN222_FinalProject
```

## Bước 5: Test Workflow

### Test 1: Tạo hợp đồng tự động
1. Đăng nhập với tài khoản buyer
2. Thêm sản phẩm vào giỏ hàng
3. Đặt hàng
4. ✅ Kiểm tra hợp đồng được tạo tự động tại `/Contracts/Index`

### Test 2: Xác nhận 2 bên
1. Đăng nhập với tài khoản buyer
2. Vào `/Contracts/Index`
3. Click vào hợp đồng → Click "Tôi xác nhận hợp đồng (Người mua)"
4. ✅ Kiểm tra `BuyerConfirmed` = true

5. Đăng nhập với tài khoản seller
6. Vào `/Contracts/Index`
7. Click vào hợp đồng → Click "Tôi xác nhận hợp đồng (Người bán)"
8. ✅ Kiểm tra `SellerConfirmed` = true
9. ✅ Kiểm tra status chuyển sang "confirmed"

### Test 3: Admin duyệt
1. Đăng nhập với tài khoản admin
2. Vào `/Admin/Contracts/Index`
3. ✅ Kiểm tra hợp đồng xuất hiện trong danh sách chờ duyệt
4. Click "Duyệt hợp đồng"
5. ✅ Kiểm tra status = "approved"
6. ✅ Kiểm tra order status = "confirmed"

### Test 4: Bàn giao xe
1. Đăng nhập với tài khoản seller
2. Vào `/Orders/Index?viewType=seller`
3. ✅ Kiểm tra nút "Bàn giao xe" chỉ hiện khi hợp đồng đã approved
4. Click "Bàn giao xe"
5. ✅ Kiểm tra order status chuyển sang "shipped"

### Test 5: Admin từ chối
1. Thực hiện Test 1 và Test 2
2. Admin vào `/Admin/Contracts/Index`
3. Click "Từ chối" và nhập lý do
4. ✅ Kiểm tra status = "rejected"
5. ✅ Kiểm tra buyer và seller có thể xem lý do từ chối
6. ✅ Kiểm tra không thể bàn giao xe

## Các Trang Quan trọng

| Trang | URL | Người dùng |
|-------|-----|------------|
| Danh sách hợp đồng | `/Contracts/Index` | Buyer, Seller |
| Chi tiết hợp đồng | `/Contracts/Details/{id}` | Buyer, Seller |
| Hợp đồng PDF | `/Orders/Contract/{orderId}` | Buyer, Seller |
| Duyệt hợp đồng | `/Admin/Contracts/Index` | Admin |
| Danh sách đơn hàng | `/Orders/Index` | Buyer, Seller |

## Troubleshooting

### Lỗi: Bảng contracts không tồn tại
**Giải pháp:** Chạy lại script `CREATE_CONTRACTS_TABLE.sql`

### Lỗi: ContractService not registered
**Giải pháp:** Thêm vào `Program.cs`:
```csharp
builder.Services.AddScoped<IContractService, ContractService>();
```

### Lỗi: Không tạo được hợp đồng tự động
**Giải pháp:** Kiểm tra `OrderService.cs` line 120-129:
```csharp
try
{
    await _contractService.CreateContractFromOrderAsync(createdOrder.Id);
}
catch (Exception ex)
{
    Console.WriteLine($"Lỗi tạo hợp đồng: {ex.Message}");
}
```

### Hợp đồng không xuất hiện trong danh sách admin
**Nguyên nhân:** Chưa đủ 2 bên xác nhận
**Giải pháp:** Đảm bảo CẢ buyer VÀ seller đã xác nhận hợp đồng

### Không thể bàn giao xe
**Nguyên nhân:** Hợp đồng chưa được admin duyệt
**Giải pháp:** Admin phải duyệt hợp đồng trước

## Files Đã Thay đổi

### Modified:
1. `BLL/Services/ContractService.cs`
   - Added `CreateContractFromOrderAsync()`
   - Added `IsContractApprovedAsync()`
   - Updated `AdminApproveAsync()` to update order status

2. `PRN222_FinalProject/Pages/Orders/Index.cshtml`
   - Added contract status display
   - Added contract approval check before vehicle handover

3. `PRN222_FinalProject/Pages/Orders/Index.cshtml.cs`
   - Added `IContractService` dependency
   - Load contract info for each order

### Created:
1. `PRN222_FinalProject/Pages/Contracts/Index.cshtml`
2. `PRN222_FinalProject/Pages/Contracts/Index.cshtml.cs`
3. `CONTRACT_WORKFLOW_GUIDE.md`
4. `SETUP_CONTRACT_FEATURE.md`

## Checklist Hoàn thành

- [x] Database tables created (contracts, contract_confirmations)
- [x] ContractService với đầy đủ methods
- [x] Tự động tạo hợp đồng khi đặt hàng
- [x] Trang xác nhận hợp đồng cho buyer/seller
- [x] Trang duyệt hợp đồng cho admin
- [x] Trang danh sách hợp đồng
- [x] Hiển thị trạng thái hợp đồng trong đơn hàng
- [x] Chặn bàn giao xe nếu chưa có hợp đồng được duyệt
- [x] Lưu lịch sử xác nhận
- [x] IP tracking
- [x] Documentation

## Kết luận

Tính năng hợp đồng số đã được tích hợp hoàn chỉnh với quy trình:

1. ✅ Tự động tạo khi đặt hàng
2. ✅ Xác nhận 2 bên (buyer & seller)
3. ✅ Admin duyệt
4. ✅ Chỉ cho phép bàn giao xe sau khi hợp đồng được duyệt

Hệ thống đảm bảo tính minh bạch và pháp lý trong giao dịch mua bán xe điện.
