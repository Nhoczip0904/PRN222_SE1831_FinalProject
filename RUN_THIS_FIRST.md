# ⚠️ LỖI: Invalid column name 'ApprovalStatus'

## 🔴 Nguyên nhân
Database chưa có các cột approval cho bảng `products`.

## ✅ Cách sửa

### Bước 1: Mở SQL Server Management Studio (SSMS)
```
1. Mở SSMS
2. Connect to: (local) hoặc localhost
3. Database: ev_battery_trading2
```

### Bước 2: Chạy SQL Script
```sql
-- Copy toàn bộ nội dung file AddProductApproval.sql và chạy
-- HOẶC mở file trong SSMS và nhấn F5
```

### Script cần chạy:
```sql
USE ev_battery_trading2;
GO

-- Add new columns
ALTER TABLE products
ADD approval_status NVARCHAR(20) NULL,
    approved_by INT NULL,
    approved_at DATETIME NULL,
    rejection_reason NVARCHAR(500) NULL;
GO

-- Set default value for existing rows
UPDATE products
SET approval_status = 'approved'
WHERE approval_status IS NULL;
GO

-- Now add default constraint
ALTER TABLE products
ADD CONSTRAINT DF_products_approval_status 
DEFAULT 'pending' FOR approval_status;
GO

-- Add check constraint
ALTER TABLE products
ADD CONSTRAINT CK_products_approval_status 
CHECK (approval_status IN ('pending', 'approved', 'rejected'));
GO

-- Add foreign key
ALTER TABLE products
ADD CONSTRAINT FK_products_approved_by 
FOREIGN KEY (approved_by) REFERENCES users(id);
GO

-- Create index
CREATE INDEX IX_products_approval_status ON products(approval_status);
GO

PRINT 'Product approval columns added successfully!';
```

### Bước 3: Verify
```sql
-- Kiểm tra các cột đã được thêm
SELECT TOP 1 * FROM products;

-- Kiểm tra constraints
SELECT * FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE
WHERE TABLE_NAME = 'products';
```

### Bước 4: Chạy lại ứng dụng
```bash
dotnet run --project PRN222_FinalProject
```

---

## 🎯 Giải thích

### Tại sao cần chạy SQL?
- Code đã có các property: `ApprovalStatus`, `ApprovedBy`, `ApprovedAt`, `RejectionReason`
- Database chưa có các cột tương ứng
- EF Core cần database có đủ cột để mapping

### Các cột sẽ được thêm:
```
products table:
├─ approval_status NVARCHAR(20) DEFAULT 'pending'
├─ approved_by INT (FK to users.id)
├─ approved_at DATETIME
└─ rejection_reason NVARCHAR(500)
```

### Sản phẩm cũ sẽ như thế nào?
- Tất cả sản phẩm cũ sẽ được set `approval_status = 'approved'`
- Vẫn hiển thị bình thường
- Không ảnh hưởng đến dữ liệu hiện tại

---

## ⚡ Quick Fix (Copy & Paste)

**Mở SSMS → New Query → Copy paste đoạn này → F5:**

```sql
USE ev_battery_trading2;
GO

ALTER TABLE products
ADD approval_status NVARCHAR(20) NULL,
    approved_by INT NULL,
    approved_at DATETIME NULL,
    rejection_reason NVARCHAR(500) NULL;
GO

UPDATE products SET approval_status = 'approved' WHERE approval_status IS NULL;
GO

ALTER TABLE products ADD CONSTRAINT DF_products_approval_status DEFAULT 'pending' FOR approval_status;
GO

ALTER TABLE products ADD CONSTRAINT CK_products_approval_status CHECK (approval_status IN ('pending', 'approved', 'rejected'));
GO

ALTER TABLE products ADD CONSTRAINT FK_products_approved_by FOREIGN KEY (approved_by) REFERENCES users(id);
GO

CREATE INDEX IX_products_approval_status ON products(approval_status);
GO

PRINT 'Done!';
```

**Sau đó chạy lại app!**

---

## 🚨 Nếu gặp lỗi "Column already exists"

Có nghĩa là đã chạy script rồi. Chỉ cần chạy lại app:
```bash
dotnet run --project PRN222_FinalProject
```

---

## ✅ Sau khi chạy SQL thành công

Ứng dụng sẽ hoạt động bình thường với:
- ✅ So sánh sản phẩm
- ✅ Đấu giá với ví
- ✅ Phê duyệt sản phẩm (backend đã có)
- ✅ Tất cả chức năng khác
