-- COPY TOÀN BỘ VÀ CHẠY TRONG SQL SERVER MANAGEMENT STUDIO
-- Database: ev_battery_trading2

USE ev_battery_trading2;
GO

-- Thêm các cột mới
ALTER TABLE products
ADD approval_status NVARCHAR(20) NULL,
    approved_by INT NULL,
    approved_at DATETIME NULL,
    rejection_reason NVARCHAR(500) NULL;
GO

-- Set tất cả sản phẩm cũ thành approved
UPDATE products
SET approval_status = 'approved',
    approved_at = created_at
WHERE approval_status IS NULL;
GO

-- Thêm default constraint
ALTER TABLE products
ADD CONSTRAINT DF_products_approval_status 
DEFAULT 'pending' FOR approval_status;
GO

-- Thêm check constraint
ALTER TABLE products
ADD CONSTRAINT CK_products_approval_status 
CHECK (approval_status IN ('pending', 'approved', 'rejected'));
GO

-- Thêm foreign key
ALTER TABLE products
ADD CONSTRAINT FK_products_approved_by 
FOREIGN KEY (approved_by) REFERENCES users(id);
GO

-- Tạo index
CREATE INDEX IX_products_approval_status ON products(approval_status);
GO

SELECT 'SUCCESS! Đã thêm các cột approval vào bảng products' AS Result;
GO

-- Kiểm tra
SELECT TOP 5 
    id, 
    name, 
    approval_status, 
    approved_at 
FROM products;
GO
