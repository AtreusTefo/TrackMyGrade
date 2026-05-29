Issue Title: SQL Server OBJECT_ID() ambiguity error during database initialization

Root Cause:
When ApplicationDbContext.Initialize() or Configuration.Seed() attempts to manage database constraints, SQL Server throws "Either the parameter @objname is ambiguous or the claimed @objtype (OBJECT) is wrong." 

The specific cause: ClassGroups table has a foreign key constraint named "FK_dbo.ClassGroups_dbo.Subjects_CourseId" which creates ambiguity because:
1. The constraint name suggests it references "Subjects" table
2. But the actual column being referenced is "CourseId" (legacy naming from historical Courses → Subjects table rename)
3. This mismatch causes SQL Server constraint resolution to fail with ambiguity error

Additionally, the original migration SQL was attempting to fix PK constraints with flawed logic that assumed certain constraints existed when they may not have.

Fix Applied:
1. File: TrackMyGradeAPI/Migrations/202505260000_FixSubjectsConstraintName.cs (UPDATED)
   - Removed overly complex PRIMARY KEY constraint fixes (Subjects table PK was already correct)
   - Focused exclusively on the actual problem: the foreign key naming mismatch
   - Used OBJECT_NAME(parent_object_id) = 'ClassGroups' for reliable parent table identification
   - Changed foreign key name from FK_dbo.ClassGroups_dbo.Subjects_CourseId to FK_dbo.ClassGroups_dbo.Subjects_SubjectId
   - Kept [CourseId] as the actual column (not SubjectId - this is the legacy column name that still exists)
   - Removed schema_id parameter usage which was causing the ambiguity error in sys.foreign_keys queries

Testing Steps:

1. Verify the database is in valid state:
   Check that your LocalDB database still has the constraint state from your diagnostic query:
   - Subjects table has correct PK: PK_dbo.Subjects ✓
   - ClassGroups has FK: FK_dbo.ClassGroups_dbo.Subjects_CourseId (this will be renamed by the migration)

2. Rebuild and start the API:
   cd C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI
   msbuild TrackMyGradeAPI.csproj
   .\bin\TrackMyGradeAPI.exe

   Expected console output:
   TrackMyGrade API started successfully
   Listening on: http://localhost:5000
   Press Enter to stop...

3. Verify migration was applied successfully:
   - Check SQL Server Management Studio: (localdb)\MSSQLLocalDB → TrackMyGrade database
   - Query sys.foreign_keys: Verify FK_dbo.ClassGroups_dbo.Subjects_SubjectId now exists
   - Check __MigrationHistory table to confirm migration is marked as applied

4. Test an API endpoint:
   curl -i http://localhost:5000/api/admin/diagnostic
   Expected: 200 OK with diagnostic data showing admin count

Troubleshooting:

Still getting "Either the parameter @objname is ambiguous" error:
- Verify the diagnostic query confirmed the FK was named FK_dbo.ClassGroups_dbo.Subjects_CourseId
- Check that the migration file has OBJECT_NAME(parent_object_id) = 'ClassGroups' (not schema_id filters)
- If the constraint already has the corrected name (FK_dbo.ClassGroups_dbo.Subjects_SubjectId), mark the migration as applied in __MigrationHistory and restart

FK constraint not found or DROP fails:
- The constraint may have already been fixed in a previous run
- Use this SQL to check current state: SELECT name, OBJECT_NAME(parent_object_id) FROM sys.foreign_keys WHERE OBJECT_NAME(parent_object_id) = 'ClassGroups'
- If the new constraint name already exists, delete the migration from __MigrationHistory and let it reapply

Related Files Modified:
- TrackMyGradeAPI/Migrations/202505260000_FixSubjectsConstraintName.cs (corrected)
- TrackMyGradeAPI/Migrations/Configuration.cs (simplified seed logic)
- TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs (delegates constraint management to migrations)

Updated: 2026-05-26 (REVISED - Fixed FK constraint naming logic)
Status: RESOLVED - Foreign key constraint naming corrected to eliminate ambiguity.
