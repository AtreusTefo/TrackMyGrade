-- Diagnostic Script: Check Migration Status and Foreign Key Constraints
-- Purpose: Verify if the FixSubjectsConstraintName migration has been applied
-- and check the current state of foreign keys on the ClassGroups table.

-- 1. Check if migration history table exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory')
BEGIN
	PRINT '=== Migration History ===';
	SELECT MigrationId, ContextKey, ProductVersion 
	FROM [dbo].[__MigrationHistory] 
	ORDER BY MigrationId DESC;
END
ELSE
BEGIN
	PRINT '*** WARNING: __MigrationHistory table does not exist. Database may not be initialized.';
END

-- 2. Check current state of foreign keys on ClassGroups
PRINT '';
PRINT '=== Current Foreign Keys on ClassGroups ===';
SELECT 
	fk.name AS ForeignKeyName,
	OBJECT_NAME(fk.parent_object_id) AS ParentTable,
	COL_NAME(fk.parent_object_id, fkc.parent_column_id) AS ParentColumn,
	OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
	COL_NAME(fk.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'ClassGroups'
ORDER BY fk.name;

-- 3. Check if the old ambiguous constraint exists
PRINT '';
PRINT '=== Checking for Ambiguous Constraint (FK_dbo.ClassGroups_dbo.Subjects_CourseId) ===';
IF EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_CourseId'
)
BEGIN
	PRINT 'STATUS: AMBIGUOUS CONSTRAINT STILL EXISTS - Migration may not have been applied';
END
ELSE
BEGIN
	PRINT 'STATUS: Ambiguous constraint does not exist';
END

-- 4. Check if the new corrected constraint exists
PRINT '';
PRINT '=== Checking for Corrected Constraint (FK_dbo.ClassGroups_dbo.Subjects_SubjectId) ===';
IF EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_SubjectId'
)
BEGIN
	PRINT 'STATUS: Corrected constraint EXISTS - Migration has been applied successfully';
END
ELSE
BEGIN
	PRINT 'STATUS: Corrected constraint does not exist - Migration may need to be applied';
END

-- 5. Check Subjects table structure
PRINT '';
PRINT '=== Subjects Table Structure ===';
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Subjects'
ORDER BY ORDINAL_POSITION;

-- 6. Check ClassGroups table structure
PRINT '';
PRINT '=== ClassGroups Table Structure ===';
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ClassGroups'
ORDER BY ORDINAL_POSITION;
