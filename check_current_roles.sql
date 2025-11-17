-- Script to check current roles in the database and existing constraints
-- Run this script in SQL Server Management Studio or via SQL command

-- Check distinct roles currently in the users table
SELECT DISTINCT role AS CurrentRoles, COUNT(*) AS UserCount
FROM dbo.users
GROUP BY role
ORDER BY role;

-- Check existing check constraints on the users table
SELECT 
    name AS ConstraintName,
    definition AS ConstraintDefinition,
    create_date,
    modify_date
FROM sys.check_constraints 
WHERE parent_object_id = OBJECT_ID('dbo.users');

-- Check all constraints on the users table
SELECT 
    c.name AS ConstraintName,
    c.type_desc AS ConstraintType,
    CASE 
        WHEN c.type = 'C' THEN cc.definition
        ELSE 'N/A'
    END AS Definition
FROM sys.objects c
LEFT JOIN sys.check_constraints cc ON c.object_id = cc.object_id
WHERE c.parent_object_id = OBJECT_ID('dbo.users')
    AND c.type IN ('C', 'F', 'UQ', 'PK') -- C=CHECK, F=FOREIGN KEY, UQ=UNIQUE, PK=PRIMARY KEY
ORDER BY c.type_desc, c.name;
