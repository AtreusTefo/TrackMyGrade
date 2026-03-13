# Daily Report — 2026-03-13

**Project:** TrackMyGrade  
**Developer:** Atreus Tefo  
**Branch:** `main`  
**Commit:** `465ebb7`

---

## What I Did Today

Designed and implemented two new student identity fields across the full stack — a
system-generated unique **Student Number** and a mandatory **Omang No. / Passport**
field — mirroring the way real tertiary institutions identify students. Work covered
every layer: database schema, EF6 model, DTOs, validation, AutoMapper, service logic,
Angular interfaces, reactive form, detail view, and list view. The session also
included diagnosing and resolving three layered production bugs that prevented the
fields from appearing at runtime.

---

## What Was Completed

### 1. Student Number — auto-generated unique identifier
- Added `StudentNumber` (`NVARCHAR(20)`) column to the `dbo.Students` table.
- Format: `STU-{YEAR}-{SEQUENCE}` (e.g. `STU-2026-0001`).
- Generation logic in `StudentService.GenerateStudentNumber()` — queries the highest
  existing sequence for the current year and increments by one, guaranteeing
  uniqueness without a separate DB sequence object.
- Treated as server-only: ignored in `StudentCreateDto → Student` and
  `StudentUpdateDto → Student` AutoMapper maps so clients can never overwrite it.
- Displayed as a colour pill badge under the student name on the detail page.
- Replaces the raw database `Id` column on the student list table.

### 2. Omang No. / Passport — mandatory identity document field
- Added `OmangOrPassport` (`NVARCHAR(9)`) column to the `dbo.Students` table.
- Included in `StudentDtoBase` (shared by create and update DTOs).
- FluentValidation rule: required, alphanumeric only (`^[a-zA-Z0-9]+$`), exactly
  9 characters — covers both the 9-digit Botswana Omang national ID and standard
  9-character passport numbers.
- Angular form: input field with `maxlength="9"`, client-side inline validation with
  matching error messages, and backfill into the edit form when loading an existing
  student.
- Displayed on the student detail page alongside Grade.

### 3. Database schema migration (additive, no data loss)
- `ApplicationDbContext.Initialize()` rewritten to use a **null EF initialiser +
  `CreateIfNotExists()` + conditional `ALTER TABLE`** pattern.
- Both columns are added with `IF NOT EXISTS` guards so the migration is idempotent
  — safe to run on every API restart.
- Existing students are backfilled with generated student numbers on first migration.

### 4. Full-stack integration verified
- End-to-end smoke test confirmed:
  `StudentNumber: STU-2026-0001  OmangOrPassport: 123456789  Total: 54`
- Swagger UI, all CRUD endpoints, and the Angular SPA tested successfully.

### 5. Code committed and pushed to GitHub
- Repository: https://github.com/AtreusTefo/TrackMyGrade
- 12 files changed, 134 insertions, 8 deletions pushed to `main`.

---

## Challenges Faced and How They Were Resolved

### Challenge 1 — `DropCreateDatabaseIfModelChanges` silently failing to update the schema

**Problem:**  
After adding `StudentNumber` and `OmangOrPassport` to the EF model and switching the
initialiser to `DropCreateDatabaseIfModelChanges`, the two new columns never appeared
in `dbo.Students`. Opening the table in VS SQL Server Object Explorer confirmed the
columns were absent.

**Root cause:**  
EF6's `DropCreateDatabaseIfModelChanges` compares a hash of the current model against
what is stored in `__MigrationHistory`. Two things silently blocked the drop:
1. An open connection from VS SQL Server Object Explorer prevented `DROP DATABASE`.
2. Stale or cached hash values caused EF to consider the model unchanged.

**Resolution:**  
Replaced the unreliable built-in initialiser with an explicit migration strategy:

```csharp
Database.SetInitializer<ApplicationDbContext>(null); // disable all EF checks
context.Database.CreateIfNotExists();                // fresh install only
context.Database.ExecuteSqlCommand("IF NOT EXISTS … ALTER TABLE …"); // additive
```

This approach preserves existing data, is safe to run on every startup, and is
immune to connection or hash issues because it never attempts a DROP.

---

### Challenge 2 — `Invalid column name 'StudentNumber'` on API startup

**Problem:**  
After implementing the `Initialize()` fix above, the API still crashed on startup
with `SqlException: Invalid column name 'StudentNumber'` wrapped in a
`TargetInvocationException`.

**Root cause:**  
SQL Server **compiles an entire T-SQL batch at parse time**, before any statement
executes. The migration batch contained both:

```sql
ALTER TABLE dbo.Students ADD StudentNumber NVARCHAR(20) …  -- adds column at runtime
…
WHERE StudentNumber = ''                                    -- validated at PARSE TIME
```

SQL Server resolved `StudentNumber` during compilation of the batch — before
`ALTER TABLE` had run — and rejected it as an unknown column.

**Resolution:**  
Wrapped the backfill `UPDATE` in `EXEC('...')` so it is compiled in a **child
execution scope** that only starts after `ALTER TABLE` has committed the new column:

```sql
ALTER TABLE dbo.Students ADD StudentNumber NVARCHAR(20) NOT NULL DEFAULT '';
EXEC('UPDATE dbo.Students SET StudentNumber = … WHERE StudentNumber = ''''')
```

`EXEC` defers compilation of the inner string until runtime, by which point the
column already exists.

---

### Challenge 3 — API process exiting immediately when launched with redirected stdin

**Problem:**  
When the API was started with `Start-Process … -RedirectStandardOutput`, the process
launched and then exited within seconds, producing no error output.

**Root cause:**  
`Program.cs` uses `Console.ReadLine()` to keep the OWIN self-host process alive.
When stdin is redirected (as it is with `-RedirectStandardOutput/-RedirectStandardError`),
`Console.ReadLine()` returns `null` immediately, causing the process to exit cleanly.

**Resolution:**  
Used `-RedirectStandardOutput` only to capture the startup log for diagnosis (which
confirmed a clean start), then re-launched with `-WindowStyle Normal` (no stdin
redirection) so the process runs interactively in its own console window.

---

*Report generated: 2026-03-13*
