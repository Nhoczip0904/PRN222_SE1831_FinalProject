-- Step 1: Add columns first
-- Run this section first, then run the update section separately

-- Add ApprovalStatus column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ApprovalStatus')
BEGIN
    ALTER TABLE Products 
    ADD ApprovalStatus NVARCHAR(50) NULL;
    PRINT 'Added ApprovalStatus column to Products table';
END
ELSE
BEGIN
    PRINT 'ApprovalStatus column already exists in Products table';
END

-- Add RejectionReason column
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

-- Add ApprovedBy column
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

-- Add ApprovedAt column
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

PRINT '=== STEP 1 COMPLETED - COLUMNS ADDED ===';
PRINT 'Now run Step 2 to update data and add constraints';
