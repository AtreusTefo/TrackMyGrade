# FIX_ERRORS — Error Log & Resolutions

> **Project:** TrackMyGrade
> **Stack:** Angular 18 (frontend) · ASP.NET Web API / EF6 (backend) · SQL Server
> **Last updated:** 2026-05-13

This document records every error encountered during development, its root cause, and how it was resolved. Entries are ordered chronologically.

---

## Table of Contents

1. [TS2792 — Cannot find module '@angular/router'](#1-ts2792--cannot-find-module-angularrouter)
2. [TS2792 (Cascade) — All imports failing including local relative paths](#2-ts2792-cascade--all-imports-failing-including-local-relative-paths)
3. [InvalidOperationException — EF6 model backing ApplicationDbContext has changed](#3-invalidoperationexception--ef6-model-backing-applicationdbcontext-has-changed)

---

## 1. TS2792 — Cannot find module '@angular/router'

**Date:** 2026-03-13
**File affected:** `StudentApp/src/app/components/student-detail/student-detail.component.ts`

### Error Message

### Error Message
```
TS2792: Cannot find module '@angular/router'. Did you mean to set the
'moduleResolution' option to 'nodenext', or to add aliases to the 'paths' option?
```

### Root Cause

`tsconfig.json` had `"moduleResolution": "node"`. Angular 18 packages use the `exports`
field in their `package.json` instead of a `main` entry point. The legacy `"node"`
resolution strategy does not read the `exports` map, so TypeScript could not locate
the type declarations inside `@angular/router` (and other Angular packages).

```json
// BEFORE — incompatible with Angular 18
"moduleResolution": "node"
```

### Fix

Changed `moduleResolution` to `"bundler"` in `StudentApp/tsconfig.json`. The
`"bundler"` strategy (introduced in TypeScript 5.0) mirrors how modern bundlers such
as esbuild (used by Angular CLI 18) resolve modules: it reads the `exports` field and
correctly resolves all Angular package entry points.

```json
// AFTER — correct for Angular 18 + TypeScript 5.5
"moduleResolution": "bundler"
```

**Verification:** Running the workspace TypeScript compiler directly produced zero errors:

```powershell
node .\node_modules\typescript\bin\tsc --noEmit -p .\tsconfig.json
# (no output = no errors)
```

---

## 2. TS2792 (Cascade) — All imports failing including local relative paths

**Date:** 2026-03-13  
**File affected:** `StudentApp/src/app/components/student-detail/student-detail.component.ts`

### Error Messages

```
TS2792: Cannot find module '@angular/router'.      Did you mean to set 'moduleResolution' to 'nodenext'...
TS2792: Cannot find module '@angular/core'.        Did you mean to set 'moduleResolution' to 'nodenext'...
TS2792: Cannot find module '@angular/common'.      Did you mean to set 'moduleResolution' to 'nodenext'...
TS2792: Cannot find module '../../services/student.service'. Did you mean to set 'moduleResolution'...
TS2792: Cannot find module '../../services/error.util'.      Did you mean to set 'moduleResolution'...
TS2792: Cannot find module '../../models'.                   Did you mean to set 'moduleResolution'...
```

### Root Cause

After applying the `"bundler"` fix from error #1, the same errors persisted in Visual
Studio's Error List — and now even local relative imports (`../../services/...`,
`../../models`) were reported as unresolvable. Relative imports are unaffected by
`moduleResolution`, so their failure was the key diagnostic clue.

**Visual Studio ships its own TypeScript language service** (separate from the
TypeScript version installed in `node_modules`). When the VS-bundled TypeScript is
older than or out of sync with the workspace TypeScript, it may not recognise newer
`moduleResolution` values such as `"bundler"` and falls back to broken behaviour,
generating false-positive errors across all imports.

Confirmed by comparing results:

| Compiler | Command | Result |
|---|---|---|
| VS built-in language service | (IntelliSense) |  6 TS2792 errors |
| Workspace TypeScript 5.5.4 | `tsc --noEmit` |  0 errors |

### Fix

The `tsconfig.json` is correct. The errors are **Visual Studio IntelliSense false
positives** caused by a stale language-service cache. Two remedies:

#### Option A — Clear the Visual Studio cache (recommended first step)

1. **Close Visual Studio completely.**
2. Delete the `.vs` folder at the solution root:

```powershell
Remove-Item -Recurse -Force "C:\<path-to-solution>\.vs"
```

3. Reopen the solution. VS rebuilds its IntelliSense index using the current `tsconfig.json`.

#### Option B — Configure VS to use the workspace TypeScript version

1. In Visual Studio go to **Tools → Options → TypeScript → General**.
2. Set **TypeScript version** to **"Use TypeScript installed in workspace"**.
3. Restart Visual Studio.

This tells the VS language service to use the TypeScript from `node_modules`
(5.5.4 in this project) instead of its own bundled copy, eliminating the version
mismatch permanently.

### Notes

- `@angular/router` and all other Angular 18 packages are correctly listed in
  `package.json` under `dependencies` and are present in `node_modules`.
- No `npm install` changes were required.
- The actual Angular CLI build (`ng build` / `ng serve`) was never broken; only
  the VS IntelliSense was affected.

---

## 3. InvalidOperationException — EF6 model backing ApplicationDbContext has changed

**Date:** 2026-05-13
**File affected:** `TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs`
**Layer:** Backend — EF6 Code First, SQL Server LocalDB

### Error Message

```
[TargetInvocationException]
Exception has been thrown by the target of an invocation.
  [InvalidOperationException]
  The model backing the 'ApplicationDbContext' context has changed since the
  database was created. Consider using Code First Migrations to update the
  database (http://go.microsoft.com/fwlink/?LinkId=238269).
```

### Root Cause

Two compounding problems caused this error:

**Problem 1 — Wrong database initializer**

`ApplicationDbContext.Initialize()` used `CreateDatabaseIfNotExists<ApplicationDbContext>()`.
This initializer:
1. Computes a hash of the EF model (entities, columns, relationships, indexes) when the database is first created and stores it in `__MigrationHistory`.
2. On every subsequent startup it recomputes the hash and compares it to the stored value.
3. If they differ — because any property, relationship, or index was added/changed in code — it throws `InvalidOperationException` instead of updating the schema.

The correct initializer for a project using Code First Migrations is `MigrateDatabaseToLatestVersion`, which applies pending migrations and keeps `__MigrationHistory` current.

**Problem 2 — Admin.Phone property not configured in OnModelCreating**

A migration (`202505082030_AddPhoneToAdmin`) added a `Phone` column (`nvarchar(20)`) to the `Admins` table. The `Admin` entity class was updated to include a `Phone` property. However, the `Admin` block inside `OnModelCreating` was never updated to configure this property with `.IsOptional().HasMaxLength(20)`.

Without explicit configuration, EF silently maps the property as `nvarchar(max)`. This created a column-type discrepancy between the running model and the schema on disk, further contributing to the hash mismatch.

### Files Changed

| File | Change |
|------|--------|
| `TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs` | Replaced initializer; added `Admin.Phone` fluent config; added using directives |

### Fix

#### Step 1 — Add required using directives

```csharp
// Add to top of ApplicationDbContext.cs
using System.Data.Entity.Migrations;
using TrackMyGradeAPI.Migrations;
```

#### Step 2 — Configure Admin.Phone in OnModelCreating

Locate the Admin entity configuration block and add the Phone property line:

```csharp
// BEFORE
admin.Property(e => e.Email).IsRequired().HasMaxLength(255)
    .HasColumnAnnotation(...);
admin.Property(e => e.Password).IsRequired().HasMaxLength(255);

// AFTER
admin.Property(e => e.Email).IsRequired().HasMaxLength(255)
    .HasColumnAnnotation(...);
admin.Property(e => e.Phone).IsOptional().HasMaxLength(20);  // Added
admin.Property(e => e.Password).IsRequired().HasMaxLength(255);
```

#### Step 3 — Replace the database initializer

```csharp
// BEFORE — throws if any model property is added or changed
Database.SetInitializer(new CreateDatabaseIfNotExists<ApplicationDbContext>());

// AFTER — runs pending migrations automatically; never throws on schema drift
Database.SetInitializer(
    new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
```

#### Step 4 — Update the fallback seed to include Phone

The inline seed in `Initialize()` was missing the `Phone` field that `Configuration.Seed()` already set:

```csharp
context.Admins.Add(new Admin
{
    FirstName = "System",
    LastName  = "Admin",
    Email     = "admin@school.com",
    Phone     = "71234567",          // Added to match Configuration.Seed()
    Password  = "<bcrypt-hash>",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
});
```

#### Step 5 — Rebuild and verify

```powershell
# From TrackMyGradeAPI\ directory
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
    TrackMyGradeAPI.csproj /p:Configuration=Debug /nologo /v:minimal

.\bin\TrackMyGradeAPI.exe
# Expected output:
#   TrackMyGrade API started successfully
#   Listening on: http://localhost:5000
```

### Verification

After applying the fix the API started cleanly:

```
[Seeding] Default admin account already exists, skipping seed.
=========================================
  TrackMyGrade API started successfully
  Listening on:  http://localhost:5000
=========================================
```

No `InvalidOperationException` was thrown. The migration ran silently on startup and updated `__MigrationHistory`.

### Prevention Rules

Apply these rules to prevent this class of error recurring:

1. **Always use `MigrateDatabaseToLatestVersion` as the EF6 initializer.** Never revert to `CreateDatabaseIfNotExists` or `DropCreateDatabaseIfModelChanges` in a project that uses explicit migrations.

2. **Whenever a property is added to an entity class, update `OnModelCreating` in the same commit.** The migration alone is not sufficient — the fluent config must also reflect the property (type, length, required/optional).

3. **After adding a property to any seeded entity, update every seed method that creates that entity.** Check both `Configuration.Seed()` and any inline seed inside `Initialize()`.

4. **After any model change, rebuild and start the API locally before committing.** A build-time error is far cheaper to fix than a runtime crash in a shared environment.

5. **If the error appears on a machine where the database already exists**, the fastest recovery is to let `MigrateDatabaseToLatestVersion` apply the pending migration. Do not delete the database unless data loss is acceptable — dropping the database on a shared or staging environment will erase all data.

### Related Files

| File | Role |
|------|------|
| `TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs` | EF6 DbContext — initializer and `OnModelCreating` configuration |
| `TrackMyGradeAPI/Migrations/Configuration.cs` | Migration configuration — `AutomaticMigrationsEnabled`, `Seed()` |
| `TrackMyGradeAPI/Migrations/202505082030_AddPhoneToAdmin.cs` | Explicit migration that added `Phone` to `dbo.Admins` |
| `TrackMyGradeAPI/Models/Student.cs` | Entity definitions including `Admin.Phone` property |

---

*End of FIX_ERRORS.md*
