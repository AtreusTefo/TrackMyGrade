-- Drop __MigrationHistory table to allow EF to recreate it
-- This is necessary when resetting migrations from scratch

DROP TABLE IF EXISTS [dbo].[__MigrationHistory];

PRINT 'Successfully dropped __MigrationHistory table.';
PRINT 'Next API startup will recreate __MigrationHistory and apply all migrations fresh.';
