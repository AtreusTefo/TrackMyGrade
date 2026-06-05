-- Validation Script: Verify FixSubjectsConstraintName migration was applied successfully

PRINT '=== VALIDATION: Migration Status and FK Constraint ===';
PRINT '';

-- 1. Check migration history
PRINT '1. Migration History (last 5 entries):';
SELECT TOP 5 MigrationId, ProductVersion FROM [dbo].[__MigrationHistory] ORDER BY MigrationId DESC;
PRINT '';

-- 2. Check for ambiguous constraint (should NOT exist)
PRINT '2. Checking for AMBIGUOUS constraint FK_dbo.ClassGroups_dbo.Subjects_CourseId:';
IF EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_CourseId'
)
BEGIN
	PRINT '   STATUS: FAILED - Ambiguous constraint still exists!';
END
ELSE
BEGIN
	PRINT '   STATUS: OK - Ambiguous constraint has been removed';
END
PRINT '';

-- 3. Check for corrected constraint (should exist)
PRINT '3. Checking for CORRECTED constraint FK_dbo.ClassGroups_dbo.Subjects_SubjectId:';
IF EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_SubjectId'
)
BEGIN
	PRINT '   STATUS: OK - Corrected constraint exists!';
END
ELSE
BEGIN
	PRINT '   STATUS: FAILED - Corrected constraint not found!';
END
PRINT '';

-- 4. List all foreign keys on ClassGroups
PRINT '4. All Foreign Keys on ClassGroups table:';
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
PRINT '';

-- 5. Summary
PRINT '=== VALIDATION COMPLETE ===';
