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
