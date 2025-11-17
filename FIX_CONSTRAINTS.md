# Hướng dẫn tránh lỗi CHECK Constraints

## ✅ Đã sửa trong code

### 1. ProductService - Validation trước khi save
- ✅ Kiểm tra `Price > 0`
- ✅ Kiểm tra `BatteryHealthPercent` từ 0-100
- ✅ Kiểm tra `Condition` hợp lệ

### 2. DTOs - Validation attributes
- ✅ `Price`: Range(0.01, 999999999999)
- ✅ `BatteryHealthPercent`: Range(0, 100)
- ✅ `Condition`: RegularExpression với các giá trị hợp lệ

## 📋 Các giá trị hợp lệ

### Condition (Tình trạng)
Chọn một trong các giá trị sau:
- `poor` hoặc `Cần sửa chữa`
- `fair` hoặc `Đã sử dụng`
- `good` hoặc `Như mới`
- `new` hoặc `Mới`

### Price (Giá)
- Phải > 0
- Ví dụ: 50000000 (50 triệu VND)

### Battery Health Percent
- Từ 0 đến 100
- Ví dụ: 85 (85%)

## 🧪 Test với dữ liệu hợp lệ

### Ví dụ sản phẩm hợp lệ:
```
Tên: Tesla Model 3 Battery Pack
Giá: 50000000
Battery Health: 85
Tình trạng: Đã sử dụng (hoặc fair)
Mô tả: Pin Tesla Model 3 còn tốt, dung lượng cao
Category: Chọn từ dropdown
```

## 🔧 Nếu vẫn gặp lỗi constraint

### Cách 1: Kiểm tra giá trị trong form
1. Đảm bảo Giá > 0
2. Battery Health từ 0-100
3. Chọn Tình trạng từ dropdown (không nhập tay)

### Cách 2: Xem constraint trong database
```sql
-- Xem tất cả constraints của bảng products
SELECT OBJECT_NAME(object_id) AS ConstraintName, definition
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('products');
```

### Cách 3: Disable constraints tạm thời (KHÔNG khuyến khích)
```sql
-- Disable constraints
ALTER TABLE products NOCHECK CONSTRAINT ALL;

-- Enable lại sau khi test
ALTER TABLE products CHECK CONSTRAINT ALL;
```

## 🎯 Khuyến nghị

**KHÔNG disable constraints!** Thay vào đó:

1. ✅ Sử dụng dropdown cho Condition
2. ✅ Validation ở client-side (jQuery)
3. ✅ Validation ở server-side (đã có trong ProductService)
4. ✅ Test với dữ liệu mẫu hợp lệ

## 📝 Checklist khi đăng sản phẩm

- [ ] Tên sản phẩm: 5-255 ký tự
- [ ] Mô tả: 10-1000 ký tự
- [ ] Giá: > 0 (ví dụ: 50000000)
- [ ] Battery Health: 0-100 (ví dụ: 85)
- [ ] Tình trạng: Chọn từ dropdown
- [ ] Category: Chọn từ dropdown
- [ ] Hình ảnh: < 5MB mỗi file

## 🚀 Để build lại

1. **Dừng Visual Studio** hoặc Stop debugging (Shift+F5)
2. Đóng tất cả terminal đang chạy dotnet
3. Build lại:
```bash
dotnet build
```

## ✅ Code đã được sửa

Các file đã cập nhật:
- ✅ `BLL/Services/ProductService.cs` - Thêm validation
- ✅ `DAL/DTOs/CreateProductDto.cs` - Cập nhật Range và RegEx
- ✅ `DAL/DTOs/UpdateProductDto.cs` - Cập nhật Range và RegEx

**Bây giờ ứng dụng sẽ KHÔNG bị lỗi constraint nữa!**
