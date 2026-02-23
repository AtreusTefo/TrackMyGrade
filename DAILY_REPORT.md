# Daily Development Report
**Project**: TrackMyGrade Full-Stack Application
**Developer**: Atreus Tefo
**Date**: Monday, 23 February 2026
**Repository**: https://github.com/AtreusTefo/TrackMyGrade

---

## What I Did Today

### 1. Diagnosed and Fixed the Entity Framework 6 + SQLite Provider Error
- Investigated the runtime error: *"No Entity Framework provider found for the ADO.NET provider with invariant name 'System.Data.SQLite'"*
- Analysed the two-layer provider model EF6 uses (ADO.NET layer + EF services layer)
- Identified that the connection string was using the wrong `providerName` (`System.Data.SQLite.EF6` instead of `System.Data.SQLite`)
- Created a code-based `SQLiteConfiguration : DbConfiguration` class to register the SQLite EF6 provider reliably in code rather than relying solely on the config file
- Applied the `[DbConfigurationType(typeof(SQLiteConfiguration))]` attribute to `ApplicationDbContext`

### 2. Fixed the SQLite Database Initialisation (Empty Database File)
- Discovered that `Database.CreateIfNotExists()` created a 0-byte database file without any tables
- Root cause: EF6's default `CreateDatabaseIfNotExists` initialiser uses SQL Server-specific DDL which SQLite silently ignores
- Disabled EF6's default database initialiser with `Database.SetInitializer<ApplicationDbContext>(null)`
- Replaced it with manual `CREATE TABLE IF NOT EXISTS` statements using SQLite-compatible DDL for both `Teachers` and `Students` tables

### 3. Fixed MSBuild RuntimeIdentifier for SQLite Native Interop
- Resolved MSBuild error: *"Your project file doesn't list 'win' as a RuntimeIdentifier"*
- Added `<RuntimeIdentifier>win</RuntimeIdentifier>` to `TrackMyGradeAPI.csproj` so MSBuild can resolve and copy the platform-specific `SQLite.Interop.dll` (x86/x64) to the output folder

### 4. Successfully Built and Ran the API
- Killed stale running instances that locked the output `.exe`
- Rebuilt with MSBuild targeting .NET Framework 4.8
- Confirmed the API starts on `http://localhost:5000`
- Tested all key endpoints:
  - `POST /api/teachers/register` → ✅ 200 OK — teacher registered successfully
  - `GET /api/students` → ✅ 200 OK — returns empty array `[]` (no students yet)

### 5. Resolved Port Conflict Error
- Encountered: *"Failed to listen on prefix 'http://localhost:5000/' because it conflicts with an existing registration"*
- Identified and killed the previous running instance of `TrackMyGradeAPI.exe` that was still holding the port

### 6. Written a Comprehensive Troubleshooting Document
- Created `TrackMyGradeAPI/TROUBLESHOOTING.md` documenting all 4 errors encountered with:
  - Exact error messages
  - Plain-English explanation of what each error means
  - Root cause analysis
  - Step-by-step solutions with code samples
  - Verification steps
  - Files changed per fix
- Updated `README.md` Troubleshooting section with a quick-reference table linking to the full guide

### 7. Set Up Git and Pushed All Changes to GitHub
- Created a `.gitignore` file at the repo root to exclude:
  - `bin/`, `obj/` — build output
  - `node_modules/`, `.angular/`, `dist/` — frontend build artifacts
  - `.vs/` — Visual Studio IDE files
  - `*.db` — SQLite database files
- Staged all meaningful source file changes
- Committed and pushed 2 commits to the `main` branch on GitHub
- Removed `obj/` build artifacts that were previously being tracked by git

---

## What Was Completed

| # | Task | Status |
|---|------|--------|
| 1 | EF6 SQLite provider error fixed | ✅ Complete |
| 2 | SQLite database tables created correctly on startup | ✅ Complete |
| 3 | MSBuild `RuntimeIdentifier` added for native SQLite DLLs | ✅ Complete |
| 4 | API builds and runs successfully on `http://localhost:5000` | ✅ Complete |
| 5 | `POST /api/teachers/register` endpoint working | ✅ Complete |
| 6 | `GET /api/students` endpoint returning `200 OK` | ✅ Complete |
| 7 | Port conflict resolved | ✅ Complete |
| 8 | `TROUBLESHOOTING.md` documentation written | ✅ Complete |
| 9 | `README.md` updated with known errors quick-reference table | ✅ Complete |
| 10 | `.gitignore` created and configured | ✅ Complete |
| 11 | All source changes committed and pushed to `main` on GitHub | ✅ Complete |
| 12 | `obj/` build artifacts removed from git tracking | ✅ Complete |

---

## Challenges Faced

### Challenge 1 — EF6 Provider Resolution Fails Silently
**Problem**: Entity Framework 6 failed to find the SQLite EF provider at runtime even though the `App.config` had the correct `<entityFramework>` section registered. The error gave no clear indication of *why* the config-based type resolution was failing.

**Why It Was Difficult**: EF6's provider resolution is a two-step process (ADO.NET invariant name → EF provider services) that is not well documented for SQLite. The failure happens silently if the `System.Data.SQLite.EF6.dll` assembly hasn't been loaded yet when EF scans the config.

**Resolution**: Bypassed fragile config-only registration entirely by creating a `DbConfiguration` subclass that registers the provider in code, guaranteeing the assembly is loaded before EF needs it.

---

### Challenge 2 — SQLite Database File Created But Empty (0 Bytes)
**Problem**: `Database.CreateIfNotExists()` appeared to succeed (no exception thrown, file was created) but the database contained no tables. Every EF query then failed with a cryptic "command definition" error rather than a clear "table does not exist" message.

**Why It Was Difficult**: The error message ("An error occurred while executing the command definition") gave no indication that the root cause was missing tables. It looked like an EF provider or mapping issue, not a database initialisation issue. The 0-byte file had to be manually inspected to discover the real problem.

**Resolution**: Disabled EF6's default database initialiser and replaced `CreateIfNotExists()` with manual `CREATE TABLE IF NOT EXISTS` DDL statements using SQLite-compatible syntax.

---

### Challenge 3 — MSBuild vs `dotnet build` for .NET Framework 4.8
**Problem**: Running `dotnet build` on the old-style `.csproj` (targeting `net4.8`) produced 108 compilation errors because `dotnet build` doesn't fully support legacy .NET Framework project formats. Switching to `msbuild` then triggered the `RuntimeIdentifier` error.

**Why It Was Difficult**: The project uses `<PackageReference>` (modern NuGet style) inside an old-style MSBuild `.csproj`, which creates an unusual hybrid that behaves differently under `dotnet build` vs `msbuild`.

**Resolution**: Used `msbuild` (not `dotnet build`) for all builds and added `<RuntimeIdentifier>win</RuntimeIdentifier>` to the `.csproj` to satisfy NuGet's native asset resolution for SQLite interop DLLs.

---

### Challenge 4 — Port Conflict Blocking API Restart
**Problem**: Rebuilding and restarting the API threw an `HttpListenerException` because the previous process was still running and holding port 5000.

**Why It Was Difficult**: The process window wasn't visible (launched via `Start-Process`), so there was no obvious indication the old instance was still running.

**Resolution**: Used `Get-Process -Name TrackMyGradeAPI | Stop-Process -Force` before every restart to ensure the port is always free.

---

### Challenge 5 — Terminal Session Path Issues
**Problem**: PowerShell terminal commands using `cd TrackMyGradeAPI` kept failing because the terminal was already inside the `TrackMyGradeAPI` directory from a previous session, causing double-nested path errors.

**Why It Was Difficult**: The terminal's working directory was not always predictable between command runs, causing intermittent failures with relative path commands.

**Resolution**: Switched to using absolute paths (`cd C:\Users\User\Desktop\TrackMyGrade`) for all critical git and build commands.

---

## Git Commits Today

| Hash | Message |
|------|---------|
| `35ad469` | `chore: remove obj/ build artifacts from tracking` |
| `e38469e` | `Fix EF6 SQLite provider, database init, add troubleshooting docs` |

---

## Files Changed Today

### New Files Added
| File | Purpose |
|------|---------|
| `.gitignore` | Excludes build output, IDE files, and node_modules from git |
| `TrackMyGradeAPI/App.config` | EF6 + SQLite provider and connection string configuration |
| `TrackMyGradeAPI/Program.cs` | Console entry point with DataDirectory setup for SQLite |
| `TrackMyGradeAPI/Data/SQLiteConfiguration.cs` | Code-based EF6 provider registration for SQLite |
| `TrackMyGradeAPI/Infrastructure/SimpleDependencyResolver.cs` | Dependency injection resolver for Web API |
| `TrackMyGradeAPI/TROUBLESHOOTING.md` | Full error documentation and solutions guide |
| `TrackMyGradeAPI/start-api.ps1` | Script to build and start the API |
| `TrackMyGradeAPI/test-api.ps1` | Script to run API endpoint tests |
| `TrackMyGradeAPI/test-get.ps1` | Script to test the GET students endpoint |
| `StudentApp/src/app/services/error.util.ts` | Frontend error utility service |
| `StudentApp/src/favicon.ico` | Application favicon |

### Modified Files
| File | Change |
|------|--------|
| `README.md` | Updated Troubleshooting section with known error quick-reference table |
| `TrackMyGradeAPI/Data/ApplicationDbContext.cs` | Disabled default initialiser, added SQLite-compatible DDL table creation, added `[DbConfigurationType]` |
| `TrackMyGradeAPI/TrackMyGradeAPI.csproj` | Added `<RuntimeIdentifier>win</RuntimeIdentifier>` |
| `TrackMyGradeAPI/Startup.cs` | Minor updates |
| `TrackMyGradeAPI/WebApiConfig.cs` | Minor updates |
| `TrackMyGradeAPI/web.config` | Minor updates |
| `TrackMyGradeAPI/Controllers/StudentsController.cs` | Minor updates |
| `StudentApp/package.json` | Dependency updates |
| `StudentApp/tsconfig.json` | TypeScript config updates |
| `StudentApp/src/app/app.routes.ts` | Route updates |
| `StudentApp/src/app/components/**` | Component updates |

---

## Next Steps / To Do

- [ ] Seed the database with sample teacher and student data for testing
- [ ] Test all remaining API endpoints (POST, PUT, DELETE students)
- [ ] Test the Angular frontend against the running API
- [ ] Implement password hashing (currently stored as plain text)
- [ ] Add JWT authentication with token expiration
- [ ] Add authorization enforcement on protected endpoints
- [ ] Write unit tests for Services layer
- [ ] Write integration tests for Controllers
- [ ] Set up CI/CD pipeline (GitHub Actions)

---

*Report generated: Monday, 23 February 2026*
