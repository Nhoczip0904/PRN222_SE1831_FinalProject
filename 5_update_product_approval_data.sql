-- Step 2: Update data and add constraints
-- Run this AFTER Step 1 has been completed successfully

-- First, set default values for existing NULL columns
UPDATE Products 
SET ApprovalStatus = 'approved' 
WHERE ApprovalStatus IS NULL;

PRINT 'Updated existing products with approval status';

-- Now add NOT NULL constraint for ApprovalStatus
ALTER TABLE Products ALTER COLUMN ApprovalStatus NVARCHAR(50) NOT NULL;
PRINT 'Added NOT NULL constraint to ApprovalStatus';

-- Add default constraint for new records
ALTER TABLE Products ADD CONSTRAINT DF_Products_ApprovalStatus DEFAULT 'pending' FOR ApprovalStatus;
PRINT 'Added default constraint for ApprovalStatus';

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

-- Show final result
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products' 
AND COLUMN_NAME IN ('ApprovalStatus', 'RejectionReason', 'ApprovedBy', 'ApprovedAt')
ORDER BY COLUMN_NAME;

-- Show sample data
SELECT TOP 5 
    Id, 
    Name, 
    ApprovalStatus, 
    RejectionReason, 
    ApprovedBy, 
    ApprovedAt
FROM Products
ORDER BY Id;

PRINT '=== STEP 2 COMPLETED - DATABASE UPDATED SUCCESSFULLY ===';
