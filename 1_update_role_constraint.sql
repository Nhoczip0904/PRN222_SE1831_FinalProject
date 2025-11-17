-- Script to update the role check constraint to include 'staff' role
-- Run this script in SQL Server Management Studio or via SQL command

-- First, check if the constraint exists
IF EXISTS (
    SELECT 1 
    FROM sys.check_constraints 
    WHERE name = 'CK__users__role__6166761E'
        AND parent_object_id = OBJECT_ID('dbo.users')
)
BEGIN
    -- Drop the existing constraint
    ALTER TABLE dbo.users DROP CONSTRAINT CK__users__role__6166761E;
    
    -- Create a new constraint that includes all valid roles
    ALTER TABLE dbo.users 
    ADD CONSTRAINT CK_users_role 
    CHECK (role IN ('admin', 'member', 'seller', 'buyer', 'staff'));
    
    PRINT 'Role constraint updated successfully to include staff role';
END
ELSE IF EXISTS (
    SELECT 1 
    FROM sys.check_constraints cc
    INNER JOIN sys.objects o ON cc.object_id = o.object_id
    WHERE o.parent_object_id = OBJECT_ID('dbo.users')
        AND cc.definition LIKE '%role%'
)
BEGIN
    -- Find and drop any role-related constraint
    DECLARE @constraint_name sysname;
    SELECT @constraint_name = cc.name
    FROM sys.check_constraints cc
    INNER JOIN sys.objects o ON cc.object_id = o.object_id
    WHERE o.parent_object_id = OBJECT_ID('dbo.users')
        AND cc.definition LIKE '%role%';
    
    EXEC('ALTER TABLE dbo.users DROP CONSTRAINT ' + @constraint_name);
    
    -- Create a new constraint that includes all valid roles
    ALTER TABLE dbo.users 
    ADD CONSTRAINT CK_users_role 
    CHECK (role IN ('admin', 'member', 'seller', 'buyer', 'staff'));
    
    PRINT 'Role constraint ' + @constraint_name + ' updated successfully to include staff role';
END
ELSE
BEGIN
    -- If the constraint doesn't exist, create it
    ALTER TABLE dbo.users 
    ADD CONSTRAINT CK_users_role 
    CHECK (role IN ('admin', 'member', 'seller', 'buyer', 'staff'));
    
    PRINT 'Role constraint created successfully';
END

-- Verify the constraint
SELECT 
    cc.name AS ConstraintName,
    cc.definition AS ConstraintDefinition,
    o.create_date,
    o.modify_date
FROM sys.check_constraints cc
INNER JOIN sys.objects o ON cc.object_id = o.object_id
WHERE o.parent_object_id = OBJECT_ID('dbo.users')
    AND cc.definition LIKE '%role%';
