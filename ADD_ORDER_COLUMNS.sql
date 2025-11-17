-- Thêm các cột mới vào bảng orders
USE ev_battery_trading2;
GO

-- Kiểm tra và thêm cột order_date
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'orders') AND name = 'order_date')
BEGIN
    ALTER TABLE orders ADD order_date DATETIME NULL;
    PRINT 'Đã thêm cột order_date';
END
ELSE
BEGIN
    PRINT 'Cột order_date đã tồn tại';
END
GO

-- Kiểm tra và thêm cột payment_method
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'orders') AND name = 'payment_method')
BEGIN
    ALTER TABLE orders ADD payment_method NVARCHAR(50) NULL;
    PRINT 'Đã thêm cột payment_method';
END
ELSE
BEGIN
    PRINT 'Cột payment_method đã tồn tại';
END
GO

-- Kiểm tra và thêm cột note
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'orders') AND name = 'note')
BEGIN
    ALTER TABLE orders ADD note NVARCHAR(500) NULL;
    PRINT 'Đã thêm cột note';
END
ELSE
BEGIN
    PRINT 'Cột note đã tồn tại';
END
GO

-- Cập nhật order_date cho các đơn hàng cũ (set = created_at)
UPDATE orders 
SET order_date = created_at 
WHERE order_date IS NULL;
GO

-- Cập nhật payment_method mặc định cho các đơn hàng cũ
UPDATE orders 
SET payment_method = 'COD' 
WHERE payment_method IS NULL;
GO

PRINT 'Hoàn thành thêm các cột vào bảng orders!';
GO
