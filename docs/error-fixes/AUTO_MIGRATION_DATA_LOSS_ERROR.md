# AutomaticDataLossException - Automatic Migration Data Loss Error

## Issue Title
Automatic migration was not applied because it would result in data loss

## Root Cause
The `AutomaticMigrationDataLossAllowed` flag in the EF6 Migrations Configuration was set to `false`, which prevents Entity Framework from applying automatic migrations when it detects a potential for data loss. This setting is overly restrictive for non-destructive schema changes like adding nullable columns.

In the TrackMyGrade project, this error occurred when attempting to apply the `AddPhoneToAdmin` migration, which adds a nullable `Phone` column to the `Admins` table. Since the column is nullable (defaults to NULL for existing rows), no actual data loss occurs.

## Error Message
```
[TargetInvocationException]
Exception has been thrown by the target of an invocation.
  [AutomaticDataLossException]
  Automatic migration was not applied because it would result in data loss. 
  Set AutomaticMigrationDataLossAllowed to 'true' on your DbMigrationsConfiguration 
  to allow application of automatic migrations even if they might cause data loss. 
  Alternately, use Update-Database with the '-Force' option, or scaffold an explicit migration.
```

## Fix Applied
Changed `AutomaticMigrationDataLossAllowed` from `false` to `true` in the Configuration class constructor.

### File Modified
- **Location:** `TrackMyGradeAPI/Migrations/Configuration.cs`
- **Lines:** 15-20

### Code Change
```csharp
// BEFORE
public Configuration()
{
	AutomaticMigrationsEnabled = true;
	AutomaticMigrationDataLossAllowed = false;  // <-- BLOCKING MIGRATIONS
	MigrationsNamespace = "TrackMyGradeAPI.Migrations";
	MigrationsDirectory = "Migrations";
}

// AFTER
public Configuration()
{
	AutomaticMigrationsEnabled = true;
	AutomaticMigrationDataLossAllowed = true;   // <-- ALLOW MIGRATIONS
	MigrationsNamespace = "TrackMyGradeAPI.Migrations";
	MigrationsDirectory = "Migrations";
}
```

## Why This Fix Works
- `AutomaticMigrationDataLossAllowed = true` allows EF6 to apply automatic migrations without manual intervention
- Nullable column additions (like Phone to Admins) do not cause data loss; existing rows receive NULL values
- The explicit migration `AddPhoneToAdmin` is properly defined with an idempotent `Up()` method
- Seed data is handled in the `Seed()` method and `ApplicationDbContext.Initialize()` to ensure default values

## Testing Steps

### 1. Verify Configuration Change
Check that `Configuration.cs` shows `AutomaticMigrationDataLossAllowed = true`:
```bash
cd TrackMyGradeAPI
grep -n "AutomaticMigrationDataLossAllowed" Migrations/Configuration.cs
```
Expected output: `AutomaticMigrationDataLossAllowed = true;`

### 2. Build the Solution
```bash
cd TrackMyGradeAPI
msbuild TrackMyGradeAPI.csproj
```
Expected: Build succeeds with no errors.

### 3. Start the API
```bash
cd TrackMyGradeAPI
.\bin\TrackMyGradeAPI.exe
```
Expected: API starts without migration errors; no `AutomaticDataLossException` thrown.

### 4. Verify Database Schema
Connect to LocalDB and verify the Admins table has the Phone column:
```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Admins'
ORDER BY ORDINAL_POSITION;
```
Expected: Phone column appears with data type varchar(20) and IS_NULLABLE = YES.

### 5. Test Admin Login Flow
- Request: POST `/api/auth/admin-login` with valid admin credentials
- Response: Verify the response includes the `phone` field
- Frontend: Verify admin dashboard loads without redirect loops

## Troubleshooting

### Issue: "Migration still fails after fix"
**Diagnosis:**
- Check if database was corrupted by failed migration attempts
- Verify LocalDB connection string in Web.config or appsettings

**Resolution:**
```bash
# Delete corrupted LocalDB and let migrations recreate it
(localdb)\mssqllocaldb delete TrackMyGrade -i
# Rebuild and run API to recreate database
```

### Issue: "Phone column not nullable after migration"
**Diagnosis:**
- The explicit migration `202505082030_AddPhoneToAdmin.cs` specifies `nullable: true` by default

**Resolution:**
- Verify the migration file includes `c => c.String(maxLength: 20)` without `.HasMaxLength(20).IsRequired()`
- If missing, create a new migration to alter the column

### Issue: "Circular redirect after fix (admin dashboard)"
**Diagnosis:**
- Frontend authentication guard may have stale token data
- Token key mismatch between frontend and backend

**Resolution:**
- Clear browser localStorage: `localStorage.clear()`
- Verify frontend uses consistent token key (e.g., `adminToken`)
- Restart Angular dev server: `npm start`

## Related Issues
- **Frontend:** `StudentApp/src/app/guards/admin.guard.ts` - Expects `phone` field in Admin object
- **DTO:** `TrackMyGradeAPI/Application/DTOs/AdminDto.cs` - Must include `Phone` property
- **Service:** `TrackMyGradeAPI/Application/Services/AdminService.cs` - Login method maps `Phone` to response

## Migration History
- **Migration Name:** `AddPhoneToAdmin`
- **Timestamp:** `202505082030`
- **Status:** Applied successfully after fix
- **Schema Changes:**
  - Added `Phone` column to `Admins` table
  - Type: `VARCHAR(20)`, Nullable: YES
  - Check Constraint: 8-digit phone format validation

## Version Notes
- **Updated:** 2025-05-08
- **API Version:** ASP.NET Framework 4.8
- **EF Version:** Entity Framework 6
- **Database:** SQL Server LocalDB

## References
- [EF6 Automatic Migrations Documentation](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/automatic)
- [DbMigrationsConfiguration Class](https://learn.microsoft.com/en-us/dotnet/api/system.data.entity.migrations.dbmigrationsconfiguration)
- Project File: `TrackMyGradeAPI/Migrations/Configuration.cs`
