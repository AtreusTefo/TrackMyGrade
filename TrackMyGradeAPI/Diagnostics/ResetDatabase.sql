-- Reset Script: Clear all tables and migration history to apply migrations fresh
-- WARNING: This will delete all data. Use only in development environments.

-- 1. Drop all foreign key constraints to allow table deletion
PRINT 'Dropping all foreign key constraints...';
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'ALTER TABLE ' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
			   ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
FROM sys.foreign_keys;

IF LEN(@sql) > 0
	EXEC sp_executesql @sql;
ELSE
	PRINT 'No foreign keys found.';

-- 2. Drop all tables in reverse dependency order
PRINT '';
PRINT 'Dropping all tables...';
DECLARE @tables TABLE (TableName NVARCHAR(128));
INSERT INTO @tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo' 
  AND TABLE_NAME NOT LIKE '__MigrationHistory'
  AND TABLE_TYPE = 'BASE TABLE';

DECLARE @tableName NVARCHAR(128);
WHILE EXISTS (SELECT 1 FROM @tables)
BEGIN
	SELECT TOP 1 @tableName = TableName FROM @tables;
	EXEC ('DROP TABLE [dbo].[' + @tableName + ']');
	DELETE FROM @tables WHERE TableName = @tableName;
	PRINT 'Dropped table: ' + @tableName;
END

-- 3. Clear migration history to force all migrations to reapply
PRINT '';
PRINT 'Clearing migration history...';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory')
BEGIN
	DELETE FROM [dbo].[__MigrationHistory];
	PRINT 'Migration history cleared.';
END
ELSE
BEGIN
	PRINT 'Migration history table does not exist (will be created by EF).';
END

PRINT '';
PRINT 'Database reset complete. All tables and migration history have been cleared.';
PRINT 'The next application startup will:';
PRINT '  1. Create __MigrationHistory table';
PRINT '  2. Apply all pending migrations in order';
PRINT '  3. Apply FixSubjectsConstraintName migration with corrected FK constraint naming';
