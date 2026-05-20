# Fix: AutomaticDataLossException on Application Startup

## Issue Title
Failed to start API with AutomaticDataLossException: Automatic migration would result in data loss

## Root Cause
Entity Framework 6 Code First Migrations detected a schema mismatch between the data model and the migration:

- **Model Configuration (DbContext):** `admin.Property(e => e.Phone).IsRequired().HasMaxLength(8);` - Phone is **required (NOT NULL)**
- **Migration Code:** `AddColumn("dbo.Admins", "Phone", c => c.String(maxLength: 20));` - Phone was added as **nullable**

This mismatch created a data loss scenario because:
1. Existing admin records in the database have no Phone value (NULL)
2. The model requires Phone to be non-nullable
3. EF could not reconcile this inconsistency, throwing `AutomaticDataLossException`

Even though `AutomaticMigrationDataLossAllowed = true` was set in `Configuration.cs`, EF6 still throws the exception because the explicit migration defines incompatible column constraints.

## Fix Applied

### File: `TrackMyGradeAPI/Migrations/202505082030_AddPhoneToAdmin.cs`

**Changed From:**
```csharp
public override void Up()
{
	AddColumn("dbo.Admins", "Phone", c => c.String(maxLength: 20));
}
```

**Changed To:**
```csharp
public override void Up()
{
	AddColumn("dbo.Admins", "Phone", c => c.String(nullable: false, maxLength: 8, defaultValue: "00000000"));
}
```

### Changes Made:
- **`nullable: false`** - Enforces NOT NULL constraint on the column
- **`maxLength: 8`** - Matches the model constraint (Phone must be 8 digits)
- **`defaultValue: "00000000"`** - Provides a valid default for existing rows, ensuring no data loss

This eliminates the schema mismatch and allows migrations to apply successfully.

## Testing Steps

### 1. Verify Build Success
```bash
cd TrackMyGradeAPI
msbuild TrackMyGradeAPI.csproj
# Expected: Build succeeded (0 errors, 0 warnings)
```

### 2. Run the API
```bash
cd TrackMyGradeAPI
.\bin\TrackMyGradeAPI.exe
# Expected: API starts successfully with message "TrackMyGrade API started successfully"
```

### 3. Verify Database Schema (SQL Server)
```sql
USE TrackMyGrade
GO

-- Check Phone column exists and is NOT NULL
SELECT COLUMN_NAME, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Admins' AND COLUMN_NAME = 'Phone'

-- Expected Output:
-- COLUMN_NAME | IS_NULLABLE | CHARACTER_MAXIMUM_LENGTH
-- Phone       | NO          | 8
```

### 4. Verify Admin Record Has Phone Value
```sql
SELECT Id, FirstName, Email, Phone
FROM Admins

-- Expected: All rows have a Phone value (default "00000000" for migrated rows)
```

## Troubleshooting

### Issue: Database Still Uses Old Schema
**Solution:** Delete the database file and let migrations recreate it.

For LocalDB:
```bash
# Stop the API
# Delete the database
rm TrackMyGradeAPI\bin\TrackMyGrade.mdf
rm TrackMyGradeAPI\bin\TrackMyGrade.ldf

# Restart the API - migrations will recreate the database with correct schema
```

### Issue: "Cannot access a disposed object" Error
**Solution:** This indicates the database was recreated while the API was running. Restart the API.

### Issue: Column "Phone" Doesn't Exist After Migration
**Solution:** Manually apply the migration using Package Manager Console:
```powershell
Update-Database -ProjectName TrackMyGradeAPI
```

## Related Files
- `TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs` (line 264) - Model defines Phone as Required
- `TrackMyGradeAPI/Migrations/Configuration.cs` - Migration configuration with AutomaticMigrationDataLossAllowed = true
- `TrackMyGradeAPI/Migrations/MIGRATION_NOTES.md` - Original migration notes

## Prevention
To avoid this issue in future migrations:
1. Always ensure migration code matches the DbContext model configuration
2. When adding required columns to tables with existing data:
   - Set `nullable: false` in AddColumn
   - Provide a `defaultValue` to populate existing rows
3. Verify migration syntax matches the DbContext entity configuration

## Version & Date
- **Fixed Date:** May 8, 2025
- **Affected Version:** Build after migration `AddPhoneToAdmin`
- **Fixed In:** Current build
