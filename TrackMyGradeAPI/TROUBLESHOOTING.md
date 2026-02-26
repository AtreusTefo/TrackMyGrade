# TrackMyGrade API — Troubleshooting Guide

This document records all errors encountered during development and their solutions.
Use this as a reference when the same or similar errors occur in the future.

---

## Table of Contents

1. [ERR-001: No Entity Framework Provider Found for System.Data.SQLite](#err-001-no-entity-framework-provider-found-for-systemdatasqlite)
2. [ERR-002: SQLite Database File Is Empty (0 Bytes) — Tables Not Created](#err-002-sqlite-database-file-is-empty-0-bytes--tables-not-created)
3. [ERR-003: Port Conflict — Failed to Listen on Prefix](#err-003-port-conflict--failed-to-listen-on-prefix)
4. [ERR-004: MSBuild RuntimeIdentifier Missing for SQLite Native Interop](#err-004-msbuild-runtimeidentifier-missing-for-sqlite-native-interop)
5. [ERR-005: FluentValidation Error Messages Not Displaying or Not Responsive](#err-005-fluentvalidation-error-messages-not-displaying-or-not-responsive)
6. [ERR-006: Input Fields Allow Invalid Characters](#err-006-input-fields-allow-invalid-characters)
7. [ERR-007: Phone Field Allows More Than 8 Digits](#err-007-phone-field-allows-more-than-8-digits)
8. [ERR-008: Teacher Registration Form Lacks Inline Validation](#err-008-teacher-registration-form-lacks-inline-validation)
9. [ERR-009: ELMAH Configuration Issues in OWIN Self-Hosted Mode](#err-009-elmah-configuration-issues-in-owin-self-hosted-mode)

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

**Last Updated**: July 2025

---

## ERR-005: FluentValidation Error Messages Not Displaying or Not Responsive

### Error Description

**Symptoms:**
- "First name is required" message never shows, only "First name must be between 2 and 50 characters"
- Error messages don't disappear when user types valid input
- Same issue affects all form fields (names, email, phone, assessments)

### Root Causes

| # | Cause | Details |
|---|-------|--------|
| 1 | **Combined validation conditions** | Single `if` statement like `if (!firstName || length < 2 || length > 50)` always shows length message even for empty fields |
| 2 | **`clearFieldError()` deletes unconditionally** | Method removes error on keystroke without checking if new value is actually valid |
| 3 | **No real-time validation** | Only validates on form submit, not during typing |

### Solution

**Frontend (Angular):**

1. **Replace `clearFieldError` with `validateField(field)`:**

```typescript
validateField(field: string): void {
  delete this.fieldErrors[field];

  switch (field) {
    case 'firstName':
      if (!this.firstName.trim()) {
        this.fieldErrors['firstName'] = 'First name is required';
      } else if (!/^[a-zA-Z '\-]+$/.test(this.firstName)) {
        this.fieldErrors['firstName'] = 'First name must contain only letters';
      } else if (this.firstName.length < 2 || this.firstName.length > 50) {
        this.fieldErrors['firstName'] = 'First name must be between 2 and 50 characters';
      }
      break;
    // ... other cases
  }
}
```

2. **Update HTML to call `validateField` on input:**

```html
<input [(ngModel)]="firstName" 
       (input)="validateField('firstName')">
<span class="field-error" *ngIf="fieldErrors['firstName']">{{ fieldErrors['firstName'] }}</span>
```

**Backend (FluentValidation):**

3. **Add `.Cascade(CascadeMode.Stop)` to prevent multiple errors:**

```csharp
RuleFor(x => x.FirstName)
    .Cascade(CascadeMode.Stop)  // Stop at first failure
    .NotEmpty().WithMessage("First name is required")
    .Matches(@"^[a-zA-Z '\-]+$").WithMessage("First name must contain only letters")
    .Length(2, 50).WithMessage("First name must be between 2 and 50 characters");
```

---

## ERR-006: Input Fields Allow Invalid Characters

### Error Description

**Symptoms:**
- First Name and Last Name fields accept numbers and symbols (e.g., "John123", "Mary@#$")
- Phone field accepts letters (e.g., "abcd1234")
- Users can type invalid characters that later fail validation

### Root Causes

| # | Cause | Details |
|---|-------|--------|
| 1 | **No keystroke filtering** | Nothing prevents invalid characters from being typed |
| 2 | **Frontend validation only checks final value** | Validation runs after invalid characters are already in the input |
| 3 | **Backend validation occurs too late** | User experience is poor when server rejects after form submission |

### Solution

**Frontend (Angular):**

1. **Add keystroke filtering methods:**

```typescript
filterLetters(event: KeyboardEvent): void {
  const allowedKeys = ['Backspace', 'Delete', 'Tab', 'Enter',
                       'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown',
                       'Home', 'End', 'Escape'];
  if (allowedKeys.includes(event.key) || event.ctrlKey || event.metaKey) return;
  if (!/^[a-zA-Z '\-]$/.test(event.key)) {
    event.preventDefault();
  }
}

filterDigits(event: KeyboardEvent): void {
  const allowedKeys = ['Backspace', 'Delete', 'Tab', 'Enter',
                       'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Escape'];
  if (allowedKeys.includes(event.key) || event.ctrlKey || event.metaKey) return;
  if (!/^\d$/.test(event.key)) {
    event.preventDefault();
  }
}
```

2. **Wire to HTML inputs:**

```html
<!-- Names: letters only -->
<input [(ngModel)]="firstName" 
       (keydown)="filterLetters($event)">

<!-- Phone: digits only -->
<input [(ngModel)]="phone" 
       (keydown)="filterDigits($event)">
```

**Backend (FluentValidation):**

3. **Add `.Matches()` rules:**

```csharp
RuleFor(x => x.FirstName)
    .Cascade(CascadeMode.Stop)
    .NotEmpty().WithMessage("First name is required")
    .Matches(@"^[a-zA-Z '\-]+$").WithMessage("First name must contain only letters")
    .Length(2, 50).WithMessage("First name must be between 2 and 50 characters");
```

---

## ERR-007: Phone Field Allows More Than 8 Digits

### Error Description

**Symptoms:**
- User can type or paste more than 8 digits in phone field
- Validation fails on submit but user already entered 10+ characters
- Poor user experience due to late validation feedback

### Root Causes

| # | Cause | Details |
|---|-------|--------|
| 1 | **No `maxlength` attribute** | HTML input has no character limit |
| 2 | **Paste operations not restricted** | User can paste long strings that exceed 8 digits |

### Solution

**Frontend (HTML):**

```html
<input type="tel" 
       [(ngModel)]="phone"
       maxlength="8"
       (keydown)="filterDigits($event)">
```

**Why `maxlength` works:**
- `maxlength` is enforced by the browser before any JavaScript runs
- Blocks both typing and paste operations that exceed the limit
- Works correctly with `type="tel"` inputs (unlike `type="number"` which ignores `maxlength`)

---

## ERR-008: Teacher Registration Form Lacks Inline Validation

### Error Description

**Symptoms:**
- Teacher registration form only shows errors after submit (flat error banner)
- No inline field-level error messages
- No real-time validation feedback
- Inconsistent UX compared to student form

### Root Causes

| # | Cause | Details |
|---|-------|--------|
| 1 | **Flat error array instead of per-field errors** | `errors: string[]` instead of `fieldErrors: {[key:string]: string}` |
| 2 | **No `validateField()` method** | Only full form validation on submit |
| 3 | **No inline error spans in HTML** | Errors only show in banner, not below each field |
| 4 | **Uses `extractErrors` instead of `extractFieldErrors`** | Server errors don't map to specific fields |

### Solution

**Convert to per-field inline validation pattern:**

1. **Update component state:**

```typescript
// Before
errors: string[] = [];

// After  
fieldErrors: { [key: string]: string } = {};
serverErrors: string[] = [];
```

2. **Add `validateField()` method:**

```typescript
validateField(field: string): void {
  delete this.fieldErrors[field];

  switch (field) {
    case 'firstName':
      if (!this.firstName.trim()) {
        this.fieldErrors['firstName'] = 'First name is required';
      } else if (!/^[a-zA-Z '\-]+$/.test(this.firstName)) {
        this.fieldErrors['firstName'] = 'First name must contain only letters';
      } else if (this.firstName.length < 2 || this.firstName.length > 50) {
        this.fieldErrors['firstName'] = 'First name must be between 2 and 50 characters';
      }
      break;
    // ... other cases including cross-field validation for confirmPassword
  }
}
```

3. **Update HTML with inline errors:**

```html
<input [(ngModel)]="firstName"
       [class.input-error]="fieldErrors['firstName']"
       (input)="validateField('firstName')">
<span class="field-error" *ngIf="fieldErrors['firstName']">{{ fieldErrors['firstName'] }}</span>
```

4. **Update error handler:**

```typescript
// Before
this.errors = extractErrors(error);

// After
const { fieldErrors, generalErrors } = extractFieldErrors(error);
this.fieldErrors = fieldErrors;
this.serverErrors = generalErrors;
```

---

## ERR-009: ELMAH Configuration Issues in OWIN Self-Hosted Mode

### Error Description

**Symptoms:**
- `web.config` contains incorrect ELMAH settings for OWIN self-hosted mode
- Documentation claims `/elmah.axd` works (it doesn't in console hosting)
- Configuration inconsistencies between `App.config` and `web.config`
- Security settings use non-standard values

### Root Causes

| # | Issue | Details |
|---|-------|--------|
| 1 | **`allowRemoteAccess="false"` instead of `"0"`** | ELMAH's `SecuritySectionHandler` only recognizes `"0"`/`"1"` or `"no"`/`"yes"` |
| 2 | **Wrong connection string in `web.config`** | Points to SQL Server LocalDB; app uses SQLite |
| 3 | **Inconsistent `size` values** | `web.config` has `size="100"`, `App.config` (runtime) has `size="500"` |
| 4 | **Overpermissive authorization** | `<allow users="*"/>` grants access to any authenticated user |
| 5 | **Missing OWIN hosting notes** | No explanation that `<system.webServer>` modules are inactive |
| 6 | **Documentation inaccuracies** | Claims `/elmah.axd`, `Global.asax.cs`, and HTTP modules work |

### Solution

**Fix `web.config`:**

```xml
<!-- Fix allowRemoteAccess format -->
<security allowRemoteAccess="0"/>

<!-- Fix connection string to match actual database -->
<add name="DefaultConnection" 
     connectionString="Data Source=|DataDirectory|TrackMyGrade.db;" 
     providerName="System.Data.SQLite"/>

<!-- Fix size to match App.config -->
<errorLog type="Elmah.MemoryErrorLog, Elmah" size="500"/>

<!-- Add OWIN hosting note -->
<!-- NOTE: The modules and handlers in <system.webServer> below are active only under IIS/IIS Express.
     They do NOT execute in OWIN self-hosted (console) mode. In this app all ELMAH logging
     is performed explicitly via ErrorLoggingConfig.LogError(). -->

<!-- Fix authorization to Admin-only -->
<authorization>
  <deny users="?"/>
  <allow roles="Admin"/>
  <deny users="*"/>
</authorization>
```

**Fix `ELMAH_SETUP.md`:**

1. **Add OWIN self-hosted warning to "Accessing the Error Log" section**
2. **Remove claims about `Global.asax.cs Application_Error` and HTTP modules**
3. **Fix `XmlFileErrorLog` path from `~/App_Data/errors` to `App_Data\errors`** (no `~/` virtual paths in console apps)
4. **Update Best Practices to recommend persistent storage instead of `/elmah.axd` monitoring**

**What works correctly:**
- `ErrorLoggingConfig.LogError()` using `ErrorLog.GetDefault(null)`
- `ElmahExceptionLogger` → `LogErrorWithMessage()` in Web API pipeline
- All controllers calling `ErrorLoggingConfig.LogError(ex)` in catch blocks
