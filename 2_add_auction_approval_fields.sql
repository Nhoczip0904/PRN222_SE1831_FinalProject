-- Add approval workflow fields to Auctions table
-- Script to add missing columns for auction approval functionality

USE [ev_battery_trading2]
GO

-- Check if Auctions table exists, if not create it first
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Auctions')
BEGIN
    PRINT 'Auctions table does not exist. Please ensure the database is properly set up.';
    -- You may need to run initial database setup first
END
ELSE
BEGIN
    PRINT 'Auctions table found. Proceeding with column additions...';
END
GO

-- Add ApprovalStatus column with default value 'pending'
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Auctions' AND COLUMN_NAME = 'ApprovalStatus')
BEGIN
    ALTER TABLE Auctions 
    ADD ApprovalStatus NVARCHAR(50) NOT NULL DEFAULT 'pending';
    PRINT 'Added ApprovalStatus column with default value "pending"';
END
ELSE
BEGIN
    PRINT 'ApprovalStatus column already exists';
END
GO

-- Add ApprovedById column (nullable foreign key to Users table)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Auctions' AND COLUMN_NAME = 'ApprovedById')
BEGIN
    ALTER TABLE Auctions 
    ADD ApprovedById INT NULL;
    PRINT 'Added ApprovedById column';
END
ELSE
BEGIN
    PRINT 'ApprovedById column already exists';
END
GO

-- Add ApprovalReason column (nullable for rejection reasons)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Auctions' AND COLUMN_NAME = 'ApprovalReason')
BEGIN
    ALTER TABLE Auctions 
    ADD ApprovalReason NVARCHAR(500) NULL;
    PRINT 'Added ApprovalReason column';
END
ELSE
BEGIN
    PRINT 'ApprovalReason column already exists';
END
GO

-- Add foreign key constraint for ApprovedById if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_Auctions_Users_ApprovedById' AND parent_object_id = OBJECT_ID('Auctions'))
BEGIN
    ALTER TABLE Auctions 
    ADD CONSTRAINT FK_Auctions_Users_ApprovedById 
    FOREIGN KEY (ApprovedById) REFERENCES Users(Id);
    PRINT 'Added foreign key constraint for ApprovedById';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint for ApprovedById already exists';
END
GO

-- Update existing auctions to have 'approved' status if they are active
-- This ensures existing active auctions continue to work
UPDATE Auctions 
SET ApprovalStatus = 'approved' 
WHERE Status = 'active' AND ApprovalStatus IS NULL;
GO

-- Update existing auctions to have 'cancelled' status if they are cancelled
UPDATE Auctions 
SET ApprovalStatus = 'cancelled' 
WHERE Status = 'cancelled' AND ApprovalStatus IS NULL;
GO

-- Set remaining auctions to 'pending' if they don't have an approval status
UPDATE Auctions 
SET ApprovalStatus = 'pending' 
WHERE ApprovalStatus IS NULL;
GO

PRINT 'Auction approval fields migration completed successfully!';
GO

-- Verify the changes
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Auctions' 
AND COLUMN_NAME IN ('ApprovalStatus', 'ApprovedById', 'ApprovalReason')
ORDER BY COLUMN_NAME;
GO

-- Show sample data to verify
SELECT TOP 5 Id, Status, ApprovalStatus, ApprovedById, ApprovalReason
FROM Auctions
ORDER BY Id;
GO
