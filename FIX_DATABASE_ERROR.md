# 🔧 SỬA LỖI: Invalid column name 'ApprovalStatus'

## ❌ Lỗi hiện tại
```
SqlException: Invalid column name 'ApprovalStatus'.
Invalid column name 'ApprovedAt'.
Invalid column name 'ApprovedBy'.
Invalid column name 'RejectionReason'.
```

## 🎯 Nguyên nhân
Database chưa có các cột mới cho tính năng phê duyệt sản phẩm.

---

## ✅ CÁCH SỬA (3 BƯỚC)

### Bước 1: Mở SQL Server Management Studio
```
1. Mở SSMS
2. Connect to: (local) hoặc localhost
3. Chọn database: ev_battery_trading2
```

### Bước 2: Chạy SQL Script

**Cách 1: Mở file**
```
File → Open → File...
→ Chọn: AddProductApproval.sql
→ Nhấn F5 (Execute)
```

**Cách 2: Copy & Paste**
```
New Query → Copy đoạn SQL bên dưới → F5
```

```sql
-- Add approval columns to products table
USE ev_battery_trading2;
GO

-- Add new columns (nullable first)
ALTER TABLE products
ADD approval_status NVARCHAR(20) NULL,
    approved_by INT NULL,
    approved_at DATETIME NULL,
    rejection_reason NVARCHAR(500) NULL;
GO

-- Update existing products to approved status
UPDATE products
SET approval_status = 'approved',
    approved_at = created_at
WHERE approval_status IS NULL;
GO

-- Now add default constraint
ALTER TABLE products
ADD CONSTRAINT DF_products_approval_status 
DEFAULT 'pending' FOR approval_status;
GO

-- Add check constraint for approval_status
ALTER TABLE products
ADD CONSTRAINT CK_products_approval_status 
CHECK (approval_status IN ('pending', 'approved', 'rejected'));
GO

-- Add foreign key for approved_by
ALTER TABLE products
ADD CONSTRAINT FK_products_approved_by 
FOREIGN KEY (approved_by) REFERENCES users(id);
GO

-- Create index for faster queries
CREATE INDEX IX_products_approval_status ON products(approval_status);
GO

PRINT 'Product approval system added successfully!';
```

### Bước 3: Chạy lại ứng dụng
```bash
# Trong terminal
dotnet run --project PRN222_FinalProject
```

---

## 🎉 Kết quả

Sau khi chạy SQL thành công, bạn sẽ thấy:
```
Product approval system added successfully!
```

Ứng dụng sẽ chạy bình thường với đầy đủ tính năng:
- ✅ Danh sách sản phẩm
- ✅ So sánh sản phẩm
- ✅ Đấu giá với ví
- ✅ Phê duyệt sản phẩm (backend)

---

## 🔍 Kiểm tra

Sau khi chạy SQL, kiểm tra bằng query:
```sql
-- Xem cấu trúc bảng products
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'products'
AND COLUMN_NAME IN ('approval_status', 'approved_by', 'approved_at', 'rejection_reason');
```

Kết quả mong đợi:
```
approval_status    | nvarchar | NO
approved_by        | int      | YES
approved_at        | datetime | YES
rejection_reason   | nvarchar | YES
```

---

## ⚠️ Lưu ý

### Nếu gặp lỗi "Column already exists"
Có nghĩa là đã chạy script rồi. Chỉ cần chạy lại app.

### Nếu gặp lỗi "Constraint already exists"
```sql
-- Xóa constraint cũ và chạy lại
ALTER TABLE products DROP CONSTRAINT IF EXISTS CK_products_approval_status;
ALTER TABLE products DROP CONSTRAINT IF EXISTS FK_products_approved_by;
ALTER TABLE products DROP CONSTRAINT IF EXISTS DF_products_approval_status;
DROP INDEX IF EXISTS IX_products_approval_status ON products;
```

### Sản phẩm cũ
- Tất cả sản phẩm hiện tại sẽ được set `approval_status = 'approved'`
- Vẫn hiển thị bình thường
- Không mất dữ liệu

---

## 📊 Giải thích chi tiết

### Tại sao cần thêm các cột này?

**Tính năng phê duyệt sản phẩm:**
1. Seller tạo sản phẩm → `approval_status = 'pending'`
2. Admin duyệt → `approval_status = 'approved'`
3. Chỉ sản phẩm `approved` mới hiển thị cho user

**Các cột:**
- `approval_status`: Trạng thái (pending/approved/rejected)
- `approved_by`: Admin nào duyệt (FK to users.id)
- `approved_at`: Thời gian duyệt
- `rejection_reason`: Lý do từ chối (nếu rejected)

### Luồng hoạt động:
```
Seller tạo sản phẩm
    ↓
approval_status = 'pending'
    ↓
KHÔNG hiển thị cho user
    ↓
Admin vào "Sản phẩm chờ duyệt"
    ↓
Duyệt → approved | Từ chối → rejected
    ↓
Nếu approved → Hiển thị cho user
```

---

## 🚀 TÓM TẮT

1. **Mở SSMS** → Connect to database
2. **Chạy SQL** → AddProductApproval.sql
3. **Chạy app** → `dotnet run`
4. **Done!** ✅

**Thời gian: < 1 phút**
