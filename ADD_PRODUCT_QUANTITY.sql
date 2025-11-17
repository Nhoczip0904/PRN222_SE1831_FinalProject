-- Add quantity column to products table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'quantity')
BEGIN
    ALTER TABLE products
    ADD quantity INT NOT NULL DEFAULT 1;
    PRINT 'Added quantity column to products table';
END
ELSE
BEGIN
    PRINT 'Quantity column already exists';
END
GO

-- Update existing products to have quantity = 1
UPDATE products
SET quantity = 1
WHERE quantity IS NULL OR quantity = 0;
GO

PRINT 'Successfully added quantity column and updated existing products';
