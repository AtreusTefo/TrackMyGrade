Issue Title: SQL Server OBJECT_ID() ambiguity error during database initialization

Root Cause:
When ApplicationDbContext.Initialize() or Configuration.Seed() attempts to create database check constraints, SQL Server throws "Either the parameter @objname is ambiguous or the claimed @objtype (OBJECT) is wrong." This occurs when OBJECT_ID() is called without the second parameter specifying object type (e.g., 'C' for CHECK constraint, 'U' for table). Some OBJECT_ID() calls in constraints creation were missing the type parameter or had inconsistent syntax, causing SQL Server to ambiguously interpret the intent.

This error is triggered during startup when:
1. Database.SetInitializer(new MigrateDatabaseToLatestVersion<...>()) runs migrations
2. Configuration.Seed() is called after migration completes
3. EnsureDataIntegrityConstraints() tries to query/drop constraints using ambiguous OBJECT_ID()

Fix Applied:
1. File: TrackMyGradeAPI/Migrations/Configuration.cs
   - Updated EnsureDataIntegrityConstraints() method
   - All OBJECT_ID() calls now use correct syntax: OBJECT_ID('ConstraintName', 'C') where 'C' = CHECK constraint type
   - Added explicit IF NOT EXISTS checks before each ALTER TABLE ADD CONSTRAINT
   - Added Admins table constraints for Phone and Email formats

2. File: TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs
   - Simplified EnsureSqlServerCheckConstraints() to no-op (delegates to Configuration.Seed)
   - Removed duplicate constraint creation logic

Testing Steps:

1. Verify the database is in valid state or delete to force fresh creation:
   If you've been testing with a stale database, it may have partial/broken constraints
   Delete the LocalDB database to force fresh migration via SQL Server Management Studio:
   - Connect to (localdb)\MSSQLLocalDB
   - Right-click 'TrackMyGrade' database, select Delete, click OK

2. Rebuild and start the API:
   cd C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI
   msbuild TrackMyGradeAPI.csproj
   .\bin\TrackMyGradeAPI.exe

   Expected console output:
   TrackMyGrade API started successfully
   Listening on: http://localhost:5000
   Press Enter to stop...

3. Verify database was created successfully:
   - Check SQL Server Management Studio: (localdb)\MSSQLLocalDB → TrackMyGrade database exists
   - Tables: Admins, Teachers, Students, ClassGroups, Subjects, Assignments, Grades, etc. should all be present
   - Check __MigrationHistory table to see applied migrations

4. Test an API endpoint:
   curl -i http://localhost:5000/api/admin/diagnostic
   Expected: 200 OK with diagnostic data showing admin count

Troubleshooting:

Still getting "Either the parameter @objname is ambiguous" error:
- The database may still be in a bad state
- Verify the database was fully deleted
- If it still exists, delete it manually in Management Studio
- After deletion, restart API and let it recreate schema fresh

Database won't delete:
- The database may be in use by another connection
- Verify no SQL Server Management Studio windows are connected to it
- Verify no other TrackMyGradeAPI processes are running
- Close all instances and try again

Related Files Modified:
- TrackMyGradeAPI/Migrations/Configuration.cs
- TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs

Updated: 2026-05-25
Status: Database initialization error documented; workaround: delete and recreate LocalDB database
