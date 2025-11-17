-- Check current constraint on orders.status
SELECT 
    OBJECT_NAME(parent_object_id) AS TableName,
    name AS ConstraintName,
    definition AS ConstraintDefinition
FROM sys.check_constraints
WHERE OBJECT_NAME(parent_object_id) = 'orders'
AND name LIKE '%status%';
GO

-- Drop ALL old constraints on status column
DECLARE @ConstraintName nvarchar(200)
DECLARE constraint_cursor CURSOR FOR 
SELECT name 
FROM sys.check_constraints 
WHERE OBJECT_NAME(parent_object_id) = 'orders' 
AND name LIKE '%status%'

OPEN constraint_cursor
FETCH NEXT FROM constraint_cursor INTO @ConstraintName

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @SQL nvarchar(max) = 'ALTER TABLE orders DROP CONSTRAINT ' + @ConstraintName
    EXEC sp_executesql @SQL
    PRINT 'Dropped constraint: ' + @ConstraintName
    FETCH NEXT FROM constraint_cursor INTO @ConstraintName
END

CLOSE constraint_cursor
DEALLOCATE constraint_cursor
GO

-- Add new constraint with correct values (lowercase)
ALTER TABLE orders
ADD CONSTRAINT CK_orders_status 
CHECK (status IN ('pending', 'confirmed', 'shipped', 'delivered', 'cancelled'));
GO

PRINT 'Successfully updated orders status constraint';
