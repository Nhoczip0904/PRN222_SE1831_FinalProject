-- Add IsSold column to products table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'is_sold')
BEGIN
    ALTER TABLE products
    ADD is_sold BIT DEFAULT 0;
END
GO

-- Create system_revenues table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[system_revenues]') AND type in (N'U'))
BEGIN
    CREATE TABLE system_revenues (
        id INT IDENTITY(1,1) PRIMARY KEY,
        order_id INT NOT NULL,
        order_amount DECIMAL(18, 2) NOT NULL,
        commission_rate DECIMAL(5, 2) NOT NULL DEFAULT 0.25,
        commission_amount DECIMAL(18, 2) NOT NULL,
        created_at DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_system_revenues_orders FOREIGN KEY (order_id) REFERENCES orders(id)
    );
END
GO

-- Update existing products to have is_sold = 0
UPDATE products
SET is_sold = 0
WHERE is_sold IS NULL;
GO

PRINT 'Successfully added is_sold column and created system_revenues table';
