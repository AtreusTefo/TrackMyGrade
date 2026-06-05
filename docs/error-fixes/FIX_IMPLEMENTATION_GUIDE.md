# SQL Server Ambiguity Error - Implementation Guide

## Issue Summary
SqlException: "Either the parameter @objname is ambiguous or the claimed @objtype (OBJECT) is wrong" occurs during TrackMyGrade API startup when EF6 migrations run.

## Root Cause Analysis
The ClassGroups table has a foreign key constraint with a misleading name:
- **Current Constraint Name:** `FK_dbo.ClassGroups_dbo.Subjects_CourseId`
- **Actual Column:** `CourseId` (legacy naming from Courses → Subjects table rename)
- **Problem:** Constraint name suggests "SubjectId" but references "CourseId" column
- **Result:** SQL Server cannot disambiguate the constraint during schema management

## Applied Fix

### Migration File: 202505260000_FixSubjectsConstraintName.cs
**Status:** CORRECTED (May 26, 2026 revision)

**What Changed:**
1. Removed flawed PRIMARY KEY constraint fixes (Subjects PK was already correct)
2. Focused on the actual problem: foreign key naming mismatch
3. Used `OBJECT_NAME(parent_object_id) = 'ClassGroups'` instead of schema_id parameters
4. Changed FK constraint name from `FK_dbo.ClassGroups_dbo.Subjects_CourseId` to `FK_dbo.ClassGroups_dbo.Subjects_SubjectId`
5. Kept `[CourseId]` as the actual referenced column

**SQL Logic:**
```sql
-- Check if the OLD constraint exists using OBJECT_NAME for reliable parent table identification
IF EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_CourseId' 
	AND OBJECT_NAME(parent_object_id) = 'ClassGroups'
)
BEGIN
	-- Drop the old constraint with misleading name
	ALTER TABLE [dbo].[ClassGroups] 
	DROP CONSTRAINT [FK_dbo.ClassGroups_dbo.Subjects_CourseId];
END

-- Create the new constraint with accurate name
IF NOT EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_SubjectId'
)
BEGIN
	ALTER TABLE [dbo].[ClassGroups] 
	ADD CONSTRAINT [FK_dbo.ClassGroups_dbo.Subjects_SubjectId] 
	FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Subjects]([Id]);
END
```

## Implementation Steps

### Step 1: Verify Migration File
- Location: `TrackMyGradeAPI/Migrations/202505260000_FixSubjectsConstraintName.cs`
- Verify the Up() method uses `OBJECT_NAME(parent_object_id) = 'ClassGroups'`
- Verify the Down() method matches in reverse

### Step 2: Remove Migration from Database History (if needed)
If the broken migration was already applied:
```sql
DELETE FROM [dbo].[__MigrationHistory] 
WHERE MigrationId = '202505260000_FixSubjectsConstraintName'
```

### Step 3: Rebuild API Project
```powershell
cd C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI
msbuild TrackMyGradeAPI.csproj
```

### Step 4: Start the API
```powershell
.\bin\TrackMyGradeAPI.exe
```

Expected output:
```
TrackMyGrade API started successfully
Listening on: http://localhost:5000
Press Enter to stop...
```

### Step 5: Verify Migration Applied
```sql
-- Check if new constraint exists
SELECT name, OBJECT_NAME(parent_object_id) as parent_table
FROM sys.foreign_keys 
WHERE OBJECT_NAME(parent_object_id) = 'ClassGroups'

-- Should return: FK_dbo.ClassGroups_dbo.Subjects_SubjectId | ClassGroups

-- Check migration history
SELECT MigrationId, CreatedTime 
FROM [dbo].[__MigrationHistory]
WHERE MigrationId LIKE '%FixSubjectsConstraintName%'
```

## Testing Verification

### Test 1: API Startup
- Start the API: `.\bin\TrackMyGradeAPI.exe`
- Verify no SqlException occurs
- Verify console shows successful startup

### Test 2: API Endpoint
```powershell
curl -i http://localhost:5000/api/admin/diagnostic
# Expected: 200 OK
```

### Test 3: Database Integrity
```sql
-- Verify referential integrity
SELECT COUNT(*) as ClassGroupCount 
FROM [dbo].[ClassGroups] cg
LEFT JOIN [dbo].[Subjects] s ON cg.CourseId = s.Id
WHERE cg.CourseId IS NOT NULL AND s.Id IS NULL
# Expected: 0 (no orphaned records)
```

## Troubleshooting

### Scenario 1: Migration Already Applied Correctly
**Symptom:** New constraint name already exists: `FK_dbo.ClassGroups_dbo.Subjects_SubjectId`

**Solution:**
```sql
-- Verify it's the correct constraint
SELECT name, OBJECT_NAME(parent_object_id), OBJECT_NAME(referenced_object_id)
FROM sys.foreign_keys
WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_SubjectId'

-- If correct, manually mark migration as applied
INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
VALUES ('202505260000_FixSubjectsConstraintName', 'TrackMyGradeAPI.Migrations.Configuration', 
	0x, '6.0.0')
```

### Scenario 2: Old Constraint Still Exists
**Symptom:** Both old and new constraint names present in sys.foreign_keys

**Solution:**
```sql
-- Manually drop the old constraint
IF EXISTS (
	SELECT 1 FROM sys.foreign_keys 
	WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_CourseId'
)
BEGIN
	ALTER TABLE [dbo].[ClassGroups] 
	DROP CONSTRAINT [FK_dbo.ClassGroups_dbo.Subjects_CourseId];
END

-- Verify only new constraint remains
SELECT name FROM sys.foreign_keys 
WHERE OBJECT_NAME(parent_object_id) = 'ClassGroups'
```

### Scenario 3: Error Still Occurs After Fix
**Symptom:** Still getting "ambiguous" error despite corrected migration

**Diagnosis:**
1. Check if another constraint exists with similar name:
```sql
SELECT name FROM sys.foreign_keys 
WHERE name LIKE '%ClassGroups%'
```

2. Check for stale metadata:
```sql
DBCC FREEPROCCACHE
DBCC DROPCLEANBUFFERS
```

3. Check EF6 tracking:
```sql
SELECT * FROM [dbo].[__MigrationHistory]
```

## Related Documentation
- Primary Fix: `docs/error-fixes/SQL-ObjectID-ambiguity-fix.md`
- Migration Notes: `TrackMyGradeAPI/Migrations/MIGRATION_NOTES.md`
- Architecture: `docs/architecture/` (for entity relationships)

## Key Lessons
1. **Constraint Naming Matters:** Names should reflect actual schema, not historical table names
2. **Use OBJECT_NAME() over sys.objects joins:** More reliable for parent table identification
3. **Avoid schema_id filters in sys.foreign_keys:** Can cause ambiguity with constraint name resolution
4. **Test migration in isolation:** Run explicit migrations separately to catch naming conflicts early
5. **Document constraint changes:** Track why constraints are renamed (e.g., table rename history)

## Version History
- **2026-05-26 (Revised):** Fixed FK constraint naming logic, focused on actual problem
- **2026-05-25 (Initial):** Identified ambiguity error root cause
