-- Script to add approval fields to Products table
-- Run this script to add missing columns for product approval workflow

-- Check if columns exist before adding
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ApprovalStatus')
BEGIN
    ALTER TABLE Products 
    ADD ApprovalStatus NVARCHAR(50) NOT NULL DEFAULT 'pending';
    PRINT 'Added ApprovalStatus column to Products table';
END
ELSE
BEGIN
    PRINT 'ApprovalStatus column already exists in Products table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'RejectionReason')
BEGIN
    ALTER TABLE Products 
    ADD RejectionReason NVARCHAR(500) NULL;
    PRINT 'Added RejectionReason column to Products table';
END
ELSE
BEGIN
    PRINT 'RejectionReason column already exists in Products table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ApprovedBy')
BEGIN
    ALTER TABLE Products 
    ADD ApprovedBy INT NULL;
    PRINT 'Added ApprovedBy column to Products table';
END
ELSE
BEGIN
    PRINT 'ApprovedBy column already exists in Products table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ApprovedAt')
BEGIN
    ALTER TABLE Products 
    ADD ApprovedAt DATETIME2 NULL;
    PRINT 'Added ApprovedAt column to Products table';
END
ELSE
BEGIN
    PRINT 'ApprovedAt column already exists in Products table';
END

-- Add foreign key constraint for ApprovedBy if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_Products_Users_ApprovedBy')
BEGIN
    ALTER TABLE Products 
    ADD CONSTRAINT FK_Products_Users_ApprovedBy 
    FOREIGN KEY (ApprovedBy) REFERENCES Users(Id);
    PRINT 'Added foreign key constraint for ApprovedBy';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint for ApprovedBy already exists';
END

-- Update existing products to have default approval status
UPDATE Products 
SET ApprovalStatus = 'approved' 
WHERE ApprovalStatus IS NULL OR ApprovalStatus = '';

PRINT 'Updated existing products with default approval status';

-- Show current status
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products' 
AND COLUMN_NAME IN ('ApprovalStatus', 'RejectionReason', 'ApprovedBy', 'ApprovedAt')
ORDER BY COLUMN_NAME;

PRINT 'Script completed successfully!';
