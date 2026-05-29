# AGENTS.md - TrackMyGrade Master Instructions

## Project Context
- **Name:** TrackMyGrade
- **Backend:** ASP.NET Framework 4.8 (C#) using OWIN Self-Host, EF6, and SQL Server LocalDB.
- **Frontend:** Angular 18 SPA.
- **Primary IDEs:** VS Code 2026, Visual Studio Community 2026.
- **Main Goal:** A full-stack system for Admins, Teachers, and Students to manage and view academic performance and assessments.

## AI Behavior Guidelines
- **No Emojis:** Do NOT use emojis in any documentation, comments, or commit messages. Keep text professional and plain-text based.
- **For Claude:** Focus on clean architecture, strict type safety, and the DTO/Service/Controller pattern.
- **For Gemini/GPT:** Be extremely concise. Avoid conversational filler.
- **General:** If logic is ambiguous, explicitly state the ambiguity and request clarification from the user in a concise format. Reference ARCHITECTURE.md before suggesting structural changes.

## Error Resolution Procedure
### When an Error Occurs or Needs Fixing:
1. **Check Existing Documentation FIRST**
   - Search `docs/error-fixes/` for the error message, error code, or related keywords
   - Check `docs/daily-reports/` for recent issues and resolutions
   - Check `docs/implementation/` for known issues and completed fixes
   - Use grep/semantic search to find if this error has been documented before

2. **Identify If Already Documented**
   - If error documentation exists, review the root cause and solution
   - If a fix was already applied, verify it was implemented correctly
   - If multiple solutions exist, choose the most recent or recommended one

3. **Apply Documented Solution**
   - Follow the exact steps outlined in the existing error documentation
   - Reference the documented fix in your response to the user
   - Link to the existing error documentation file

4. **If Error Not Documented**
   - Proceed with analysis and implementation
   - Create comprehensive error documentation in `docs/error-fixes/` 
   - Include root cause, solution, testing steps, and troubleshooting guide
   - Reference this file for future occurrences

5. **If Documented Solution Doesn't Work**
   - Test the documented solution thoroughly to verify it truly doesn't resolve the issue
   - Analyze why the documented fix failed (environment differences, code changes, etc.)
   - Implement a new solution using root cause analysis
   - Update the original error documentation file with:
     - **New Section:** "Why Previous Solution Failed" - Explain the reason
     - **Revised Solution:** Replace old fix with new, tested fix
     - **Updated Testing Steps:** Reflect the new solution validation
     - **Version Note:** Add timestamp "Updated: [DATE]" at top of document
     - **Related Issues:** Link any new error files if multiple fixes discovered
   - Document both solutions if both are valid for different scenarios
   - Alert the user that documentation has been revised

### Error Documentation Template
When creating new error fix documentation:
- **Issue Title:** Clear, searchable error description
- **Root Cause:** Technical explanation of why error occurred
- **Fix Applied:** Exact changes made (file paths, line numbers, code)
- **Testing Steps:** How to verify the fix works
- **Troubleshooting:** Additional diagnostics if error persists
- **Related Files:** All files affected by the fix

## Documentation Standards
- **Style:** Professional, technical, and objective. 
- **Format:** Use standard Markdown (headings, tables, lists).
- **Prohibition:** Strictly zero emojis allowed in `.md` files.
- **Organization:** All documentation files MUST be created in their rightful folders under `docs/`:
  - `docs/architecture/` - System design, data flow, architectural patterns
  - `docs/implementation/` - Implementation guides, code summaries, completion reports
  - `docs/project/` - Project requirements, planning, deliverables, scope documentation
  - `docs/guides/` - Quick start guides, testing guides, how-to documentation
  - `docs/api-postman/` - API testing, Postman workflows, integration guides
  - `docs/error-fixes/` - Bug fixes, error resolutions, issue tracking
  - `docs/daily-reports/` - Daily progress reports and status updates
- **Never** leave documentation files in the `docs/` root directory.

## Coding Standards & Patterns
- **Backend (C#):**
  - Use **FluentValidation** for models.
  - Map entities to **DTOs** using AutoMapper.
  - Logic belongs in the **Service Layer**, not Controllers.
- **Frontend (Angular 18):**
  - Use **CanActivateFn** for route guards.
  - Component location: `StudentApp/src/app/components/`.
  - Ensure all services and components use consistent token key names (camelCase preferred, e.g., 'adminToken' not 'admin_token') for frontend authentication with localStorage. Token key mismatches can cause redirect loops. Use a shared constant for key names across all files.
- **Naming:** PascalCase for C#; camelCase for JSON and TypeScript.

## Project Structure Reference
- **API Logic:** `TrackMyGradeAPI/Application/Services/`
- **API Controllers:** `TrackMyGradeAPI/Presentation/Controllers/`
- **Frontend Components:** `StudentApp/src/app/components/`
- **Documentation:** `docs/` (Refer to `DOCUMENTATION_INDEX.md`).

## Environment Commands
- **Build API:** `cd TrackMyGradeAPI && msbuild TrackMyGradeAPI.csproj`
- **Restore NuGet Packages:** `cd TrackMyGradeAPI; msbuild TrackMyGradeAPI.csproj /t:Restore`
- **Run API:** `cd TrackMyGradeAPI; .\bin\TrackMyGradeAPI.exe`
- **Build Angular:** `cd StudentApp && npm run build`
- **Run Angular:** `cd StudentApp && npm start`

## Data Integrity, Referential Integrity & Consistency Standards

### Frontend Layer (Angular 18) - Input Validation & Sanitization
1. **Input Validation (Client-Side)**
   - Use reactive forms with validators matching backend FluentValidation rules exactly.
   - Validate input types, lengths, formats, and ranges before API submission.
   - Disable submit buttons until all validation passes; show real-time error messages.
   - Implement custom validators for cross-field validation (e.g., endDate > startDate).

2. **Input Sanitization & Security**
   - Sanitize all user input using Angular DomSanitizer to prevent XSS attacks.
   - Strip whitespace and normalize data before transmission to backend.
   - Use HttpClient interceptors for consistent request/response header handling.

3. **Client-Side Validation Limitations**
   - Treat client-side validation as UX improvement only; server validation is mandatory.
   - Never trust client-side validation alone for security or data integrity.
   - Always expect backend to reject invalid data independently.

### Backend Layer (C# / ASP.NET) - Request Processing & Business Logic
4. **Service Layer Input Validation**
   - Validate ALL incoming DTOs using FluentValidation validators in Service layer before processing.
   - Return `400 Bad Request` with detailed validation error messages on failure.
   - Include business logic validation: verify FK references exist, check permissions, enforce state transitions.

5. **Foreign Key Constraint Enforcement**
   - Before INSERT/UPDATE: Verify parent records exist for all FK values (StudentId, ClassGroupId, TeacherId).
   - Query parent tables explicitly; do NOT rely solely on EF6 cascade behavior.
   - Return `409 Conflict` if parent record missing: "StudentId 123 does not exist."
   - Prevent orphaned records at Service layer before database operation.

6. **Business Logic Encapsulation**
   - Implement all business logic in Service layer; Controllers only handle HTTP concerns.
   - Services enforce state machine transitions (e.g., Grade: Draft → Submitted → Graded).
   - Services validate domain constraints (e.g., GradeValue 0-100, DueDate >= CreatedDate).

7. **Transaction Management**
   - Wrap multi-step operations in `DbContext.Database.BeginTransaction()`.
   - Use try-catch-finally to ensure rollback on failure: `transaction.Rollback()`.
   - Commit transaction only after all business logic succeeds: `transaction.Commit()`.
   - Example: Creating Assignment + StudentEnrollment + AuditLog should be atomic.

8. **Consistent Error Responses**
   - Return proper HTTP status codes: `200 OK`, `201 Created`, `400 Bad Request`, `409 Conflict`, `422 Unprocessable Entity`.
   - Include meaningful error messages that guide frontend recovery (not internal stack traces).
   - Provide updated entity data in success responses for optimistic UI updates.

9. **Audit Trail & Logging**
   - Log ALL mutations to AuditLog table: UserId, EntityType, Operation (Create/Update/Delete), Timestamp, OldValue, NewValue.
   - Audit entries created BEFORE database commit (if commit fails, log still records intent).
   - Include user context in logs for compliance and debugging.

### Database Layer (SQL Server / EF6) - Schema Constraints & Referential Integrity
10. **Column Constraints**
    - Define NOT NULL constraints on required columns; use `[Required]` data annotation in entities.
    - Set appropriate SQL Server column types: `varchar(255)` for Email, `int` for StudentId, `datetime2` for timestamps.
    - Use check constraints for valid ranges: `[Range(0, 100)]` for GradeValue, `[Range(0.0, 4.0)]` for GPA.

11. **Uniqueness Constraints**
    - Enforce unique constraints on Email, UserName, SubjectCode using `[Index(IsUnique = true)]` data annotation.
    - Prevent duplicate enrollments: Add composite unique constraint on StudentId + ClassGroupId.
    - Duplicates detected at database level; backend returns `409 Conflict` on violation.

12. **Explicit Foreign Key Configuration**
    - Configure ALL FK relationships explicitly in `OnModelCreating()` using fluent API.
    - Define cascade behavior explicitly: `WillCascadeOnDelete(true)` or `WillCascadeOnDelete(false)`.
    - Example: Teacher deletion cascades to ClassGroups; ClassGroup deletion restricts if Assignments exist.
    - Index all FK columns for query performance.

13. **Concurrency Control**
    - Use `[Timestamp]` or `[ConcurrencyCheck]` on critical entities: Grade, Assignment, StudentEnrollment, Submission.
    - EF6 detects concurrent modifications; throw `DbUpdateConcurrencyException` on conflict.
    - Service layer handles exception: Log conflict, notify frontend, allow user to retry/merge.

14. **Soft Delete Strategy**
    - Add `IsDeleted` flag (bool, default false) to grading data: Grade, Assignment, Submission, AuditLog.
    - Hard delete only temporary data with no compliance requirement.
    - Query filters always include `&& IsDeleted == false` unless explicitly querying deleted records.
    - Soft deletes preserve audit trail and historical data for compliance.

### Cross-Layer Consistency Guarantees

15. **Timestamp Synchronization**
    - All timestamps use `DateTime.UtcNow` (server timezone, never client time).
    - Store in SQL Server as `datetime2` type.
    - Validate: CreatedAt <= UpdatedAt always.
    - Frontend converts UTC to local time for display only.

16. **Status Transition Validation**
    - Grade status follows state machine: Draft → Submitted → Graded (no backtracking).
    - Assignment status: Open → Due → Closed (dates enforce transitions).
    - Service layer validates transitions; reject invalid transitions with meaningful error.

17. **No Single-Layer Trust**
    - Frontend validation is UX-only; never skip server validation.
    - Backend validation is mandatory; never assume client sent valid data.
    - Database constraints are enforcement layer; never assume EF6 cascade alone.
    - All three layers enforce same rules independently.

### Testing Requirements
18. **Comprehensive Test Coverage**
    - Frontend: Validation rejects invalid inputs; submit button disabled until form valid.
    - Backend: Invalid DTOs rejected with `400 Bad Request`; FK constraints prevent orphans.
    - Database: Cascade operations succeed/fail as configured; unique constraints prevent duplicates.
    - Audit: Log entries capture all mutations with correct context.
    - Concurrency: Concurrent operations handled without data corruption.

## Critical Rules
1. **Headers:** Student endpoints REQUIRE `X-TeacherId` or `X-StudentToken` headers.
2. **Database:** EF6 `ApplicationDbContext.Initialize()` handles schema on startup.
3. **Logging:** Use **ELMAH** patterns for exception handling.
4. **Data Integrity:** Guarantee #17 - No layer trusts another; all enforce rules independently.
5. **Transactions:** Guarantee #7 - Multi-step operations MUST be atomic.
6. **Audit Trail:** Guarantee #9 - Every CREATE/UPDATE/DELETE MUST log to AuditLog.
7. **Referential Integrity:** Guarantee #12 - All FK relationships configured explicitly with cascade rules.