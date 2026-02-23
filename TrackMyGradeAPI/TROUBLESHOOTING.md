# TrackMyGrade API — Troubleshooting Guide

This document records all errors encountered during development and their solutions.
Use this as a reference when the same or similar errors occur in the future.

---

## Table of Contents

1. [ERR-001: No Entity Framework Provider Found for System.Data.SQLite](#err-001-no-entity-framework-provider-found-for-systemdatasqlite)
2. [ERR-002: SQLite Database File Is Empty (0 Bytes) — Tables Not Created](#err-002-sqlite-database-file-is-empty-0-bytes--tables-not-created)
3. [ERR-003: Port Conflict — Failed to Listen on Prefix](#err-003-port-conflict--failed-to-listen-on-prefix)
4. [ERR-004: MSBuild RuntimeIdentifier Missing for SQLite Native Interop](#err-004-msbuild-runtimeidentifier-missing-for-sqlite-native-interop)

---

## ERR-001: No Entity Framework Provider Found for System.Data.SQLite

### Error Message

```
System.InvalidOperationException:
No Entity Framework provider found for the ADO.NET provider with invariant name
'System.Data.SQLite'. Make sure the provider is registered in the 'entityFramework'
section of the application config file.
See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.
```

### What It Means

Entity Framework 6 uses a **two-layer provider model**:

1. **ADO.NET layer** — `System.Data.SQLite.SQLiteFactory` handles raw database connections.
2. **EF layer** — `System.Data.SQLite.EF6.SQLiteProviderServices` translates EF operations into SQLite SQL.

This error means EF found the ADO.NET provider (`System.Data.SQLite`) but **cannot resolve
the matching EF6 provider service**. The `System.Data.SQLite.EF6.dll` assembly either isn't
loaded yet or the config-file type mapping fails silently.

### Root Causes

| # | Cause | Details |
|---|-------|---------|
| 1 | **Wrong `providerName` in connection string** | Using `providerName="System.Data.SQLite.EF6"` instead of `providerName="System.Data.SQLite"`. The connection string must reference the **base ADO.NET invariant name**. |
| 2 | **Config-only provider registration is fragile** | If `System.Data.SQLite.EF6.dll` isn't loaded when EF reads the `<entityFramework>` config section, the type resolution fails silently. |
| 3 | **Missing `System.Data.SQLite.EF6.dll` in output** | The assembly isn't copied to the `bin/` folder, so EF can't instantiate `SQLiteProviderServices`. |

### Solution

**Step 1 — Fix `App.config` connection string `providerName`:**

```xml
<!-- WRONG -->
<add name="DefaultConnection"
     connectionString="Data Source=|DataDirectory|TrackMyGrade.db;"
     providerName="System.Data.SQLite.EF6"/>

<!-- CORRECT -->
<add name="DefaultConnection"
     connectionString="Data Source=|DataDirectory|TrackMyGrade.db;"
     providerName="System.Data.SQLite"/>
```

**Step 2 — Create a code-based `DbConfiguration` class (recommended by Microsoft):**

This registers the provider **in code**, guaranteeing the assembly is loaded before EF needs it.

```csharp
// Data/SQLiteConfiguration.cs
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SQLite;
using System.Data.SQLite.EF6;

namespace TrackMyGradeAPI.Data
{
    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite",
                (DbProviderServices)SQLiteProviderFactory.Instance.GetService(
                    typeof(DbProviderServices)));
        }
    }
}
```

**Step 3 — Apply `[DbConfigurationType]` to your DbContext:**

```csharp
[DbConfigurationType(typeof(SQLiteConfiguration))]
public class ApplicationDbContext : DbContext
{
    // ...
}
```

**Step 4 — Keep the `App.config` `<entityFramework>` section as a fallback:**

```xml
<entityFramework>
  <providers>
    <provider invariantName="System.Data.SQLite"
              type="System.Data.SQLite.EF6.SQLiteProviderServices, System.Data.SQLite.EF6"/>
    <provider invariantName="System.Data.SQLite.EF6"
              type="System.Data.SQLite.EF6.SQLiteProviderServices, System.Data.SQLite.EF6"/>
  </providers>
</entityFramework>
```

### Files Changed

- `TrackMyGradeAPI/App.config` — Fixed `providerName`
- `TrackMyGradeAPI/Data/SQLiteConfiguration.cs` — Created (code-based provider registration)
- `TrackMyGradeAPI/Data/ApplicationDbContext.cs` — Added `[DbConfigurationType]` attribute

---

## ERR-002: SQLite Database File Is Empty (0 Bytes) — Tables Not Created

### Error Message

```
System.Data.Entity.Core.EntityCommandExecutionException:
An error occurred while executing the command definition.
See the inner exception for details.
```

This occurs on any EF query (e.g., `_dbContext.Students.Where(...).ToList()`). The database
file exists but contains **no tables**.

### What It Means

EF6's default database initializer (`CreateDatabaseIfNotExists<T>`) uses **SQL Server-specific
DDL** to create tables. SQLite does not support these SQL Server commands, so `Database.CreateIfNotExists()`
creates the file but **silently fails** to generate tables.

### Root Cause

```csharp
// This does NOT work with SQLite — creates an empty file
context.Database.CreateIfNotExists();
```

EF6 internally generates SQL like `CREATE DATABASE`, `sys.databases`, and other SQL Server
system queries that SQLite doesn't understand.

### Solution

**Disable the default initializer** and create tables manually with SQLite-compatible DDL:

```csharp
public static void Initialize()
{
    Database.SetInitializer<ApplicationDbContext>(null);

    using (var context = new ApplicationDbContext())
    {
        context.Database.ExecuteSqlCommand(@"
            CREATE TABLE IF NOT EXISTS Teachers (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName TEXT,
                LastName  TEXT,
                Email     TEXT    NOT NULL,
                Phone     TEXT,
                Subject   TEXT,
                Password  TEXT,
                Token     TEXT
            );
        ");

        context.Database.ExecuteSqlCommand(@"
            CREATE TABLE IF NOT EXISTS Students (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                TeacherId   INTEGER NOT NULL,
                FirstName   TEXT,
                LastName    TEXT,
                Email       TEXT    NOT NULL,
                Phone       TEXT,
                Grade       INTEGER NOT NULL DEFAULT 0,
                Assessment1 INTEGER NOT NULL DEFAULT 0,
                Assessment2 INTEGER NOT NULL DEFAULT 0,
                Assessment3 INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY (TeacherId) REFERENCES Teachers(Id) ON DELETE CASCADE
            );
        ");
    }
}
```

Also add this in `OnModelCreating` to prevent EF from trying its own initialization:

```csharp
protected override void OnModelCreating(DbModelBuilder modelBuilder)
{
    Database.SetInitializer<ApplicationDbContext>(null);
    // ... rest of model configuration
}
```

### How to Verify

Check the database file size after startup:

```powershell
dir TrackMyGradeAPI\bin\TrackMyGrade.db
# Should show a non-zero file size (typically 12KB+ with empty tables)
```

If the file is 0 bytes, tables were not created — apply the fix above.

### Important: Adding New Tables

When adding new entities/models in the future, you **must** manually add a corresponding
`CREATE TABLE IF NOT EXISTS` statement in the `Initialize()` method. EF6 Code First
migrations do not work with SQLite out of the box.

### Files Changed

- `TrackMyGradeAPI/Data/ApplicationDbContext.cs` — Disabled default initializer, added manual DDL

---

## ERR-003: Port Conflict — Failed to Listen on Prefix

### Error Message

```
[HttpListenerException]
Failed to listen on prefix 'http://localhost:5000/' because it conflicts with
an existing registration on the machine.
```

### What It Means

Another process is already listening on port 5000. This is typically a **previous instance**
of `TrackMyGradeAPI.exe` that wasn't stopped before starting a new one.

### Solution

**Option 1 — Kill the existing process:**

```powershell
# Find and kill the old process
Get-Process -Name TrackMyGradeAPI -ErrorAction SilentlyContinue | Stop-Process -Force

# Verify port is free
netstat -ano | findstr ":5000"

# Start fresh
Start-Process "TrackMyGradeAPI\bin\TrackMyGradeAPI.exe"
```

**Option 2 — Find what's using the port:**

```powershell
# Find the PID using port 5000
netstat -ano | findstr ":5000"

# Identify the process
tasklist /FI "PID eq <PID>"

# Kill it
taskkill /PID <PID> /F
```

**Option 3 — Change the port in `Program.cs`:**

```csharp
const string baseUrl = "http://localhost:5001"; // Use a different port
```

### Prevention

Always stop the running instance before rebuilding and restarting:

```powershell
Get-Process -Name TrackMyGradeAPI -ErrorAction SilentlyContinue | Stop-Process -Force
msbuild TrackMyGradeAPI.csproj /t:Build /p:Configuration=Debug
Start-Process "bin\TrackMyGradeAPI.exe"
```

### Files Changed

- No code changes needed — operational fix only.

---

## ERR-004: MSBuild RuntimeIdentifier Missing for SQLite Native Interop

### Error Message

```
error : Your project file doesn't list 'win' as a "RuntimeIdentifier". You should
add 'win' to the "RuntimeIdentifiers" property in your project file and then re-run
NuGet restore.
```

### What It Means

The `System.Data.SQLite.Core` NuGet package includes **native interop DLLs** (`SQLite.Interop.dll`)
that are platform-specific (x86/x64). MSBuild needs to know the target runtime to resolve
and copy these native binaries. Without a `RuntimeIdentifier`, NuGet can't determine which
native assets to include.

### Solution

Add `<RuntimeIdentifier>win</RuntimeIdentifier>` to the main `<PropertyGroup>` in the `.csproj`:

```xml
<PropertyGroup>
  <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
  <FileAlignment>512</FileAlignment>
  <RuntimeIdentifier>win</RuntimeIdentifier>   <!-- ADD THIS -->
</PropertyGroup>
```

Then restore and rebuild:

```powershell
dotnet restore
msbuild TrackMyGradeAPI.csproj /t:Build /p:Configuration=Debug
```

### Verify Native DLLs Are Copied

After building, check that the interop DLLs exist:

```powershell
dir bin\x64\SQLite.Interop.dll
dir bin\x86\SQLite.Interop.dll
```

Both files must be present for SQLite to work at runtime.

### Files Changed

- `TrackMyGradeAPI/TrackMyGradeAPI.csproj` — Added `<RuntimeIdentifier>win</RuntimeIdentifier>`

---

## Quick Reference: Common Build & Run Commands

```powershell
# 1. Kill any running instance
Get-Process -Name TrackMyGradeAPI -ErrorAction SilentlyContinue | Stop-Process -Force

# 2. Restore NuGet packages
dotnet restore

# 3. Build
msbuild TrackMyGradeAPI.csproj /t:Build /p:Configuration=Debug

# 4. Run
Start-Process "bin\TrackMyGradeAPI.exe"

# 5. Test the API
$req = [System.Net.WebRequest]::Create("http://localhost:5000/api/students")
$req.Method = "GET"
$req.Headers.Add("X-TeacherId", "1")
$resp = $req.GetResponse()
$body = (New-Object System.IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
Write-Host $body
```

---

## Quick Reference: Config File Checklist

When troubleshooting EF6 + SQLite issues, verify all of these in `App.config`:

- [ ] `<configSections>` declares the `entityFramework` section
- [ ] Connection string uses `providerName="System.Data.SQLite"` (NOT `System.Data.SQLite.EF6`)
- [ ] `<entityFramework><providers>` maps `System.Data.SQLite` to `SQLiteProviderServices`
- [ ] `<system.data><DbProviderFactories>` registers `SQLiteFactory` under `System.Data.SQLite`
- [ ] `<remove>` tags appear before `<add>` tags in `DbProviderFactories` (prevents duplicates)
- [ ] `SQLiteConfiguration.cs` exists with code-based provider registration
- [ ] `ApplicationDbContext` has `[DbConfigurationType(typeof(SQLiteConfiguration))]`
- [ ] `Database.SetInitializer<ApplicationDbContext>(null)` is called before any DB access

---

## Frontend: Create Student Stuck On "Saving..."

**Symptom**

- On the Angular student form, clicking `Create` changes the button text to `Saving...` and it never completes. No navigation occurs, and the browser Network tab may show no request or an aborted one.

**Root Cause**

- The backend originally serialized JSON using PascalCase (`Id`, `FirstName`, `Token`).
- The Angular `Teacher` and `Student` interfaces use camelCase (`id`, `firstName`, `token`).
- After login, the raw teacher object from the backend was stored in `localStorage` without normalization.
- `StudentService.getHeaders()` called `teacher.id.toString()` even when `teacher.id` was `undefined`, throwing a synchronous `TypeError` before the HTTP `post`/`put` observable was subscribed.
- Because the exception occurred before `subscribe(...)` registered handlers, `StudentFormComponent.onSubmit()` never hit the `next` or `error` callbacks, and `isSubmitting` stayed `true`, leaving the button stuck on `Saving...`.

**Fix**

Backend:

- In `WebApiConfig.cs`, configure camelCase JSON serialization so API responses match the Angular models:

  ```csharp
  config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling =
      Newtonsoft.Json.NullValueHandling.Ignore;
  config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
      new CamelCasePropertyNamesContractResolver();
  ```

Frontend:

- In `AuthService.setCurrentTeacher(...)` and `getStoredTeacher()`, normalize both PascalCase and camelCase keys when reading/writing the teacher object so `teacher.id` and `teacher.token` are always present.
- In `StudentService.getHeaders()`, guard against `null`/`undefined` IDs:

  ```ts
  const teacher = this.authService.getCurrentTeacher();
  let headers = new HttpHeaders();
  if (teacher?.id != null) {
    headers = headers.set('X-TeacherId', teacher.id.toString());
  }
  return headers;
  ```

- In `StudentFormComponent.onSubmit()`, wrap the service call in a `try/catch` and always reset `isSubmitting` in both success and error paths so any unexpected synchronous error cannot leave the UI stuck:

  ```ts
  this.isSubmitting = true;
  try {
    // create or update via StudentService...
  } catch {
    this.errors = ['An unexpected error occurred. Please try logging out and back in.'];
    this.isSubmitting = false;
  }
  ```

- Update all remaining `subscribe(success, error)` calls to the `subscribe({ next, error })` object form to avoid deprecated patterns with RxJS 7.

Environment / Tooling:

- For Angular 18 with TypeScript 5+, ensure `tsconfig.json` uses `"moduleResolution": "bundler"` so the compiler can resolve Angular's ESM packages via their `exports` field.
- In Visual Studio, add the `StudentApp.esproj` to the solution so the IDE uses the workspace TypeScript version (from `node_modules`) instead of the older built-in tools, which do not understand `moduleResolution: "bundler"`.

**Verification Steps**

1. Rebuild the backend (`TrackMyGradeAPI`) and start `TrackMyGradeAPI.exe` on `http://localhost:5000`.
2. Stop and restart `ng serve` for the Angular app.
3. Clear browser `localStorage` for `http://localhost:4200` to remove any stale PascalCase teacher objects.
4. Register or log in as a teacher.
5. Open the student form, fill it out, and click `Create`.
6. The request should complete, the button text should revert from `Saving...`, and the app should navigate to the student list/detail view with the new record visible.

---

**Last Updated**: February 2026
