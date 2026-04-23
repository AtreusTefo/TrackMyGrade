# Daily Development Report

| | |
|---|---|
| **Date** | March 24, 2026 |
| **Project** | TrackMyGrade |
| **Developer** | Atreus Tefo |
| **Repository** | https://github.com/AtreusTefo/TrackMyGrade |
| **Branch** | `main` |
| **Backend** | ASP.NET Web API — `TrackMyGradeAPI` |
| **Frontend** | Angular 17 — `StudentApp` |

---

## 1. What I Did Today

Implemented and corrected the **Student Authentication System**, enabling students to log in and
view their grades and dashboard.

The core business rule driving all decisions today:

> **Students do not self-register. Teachers create student accounts — including login
> credentials — when adding a student. Students then log in using the email and password
> provided by their teacher.**

This required a full audit of both the backend (`TrackMyGradeAPI`) and the Angular frontend
(`StudentApp`), followed by targeted changes to fix incorrect design, remove dead code, and
wire up the correct authentication flow end-to-end.

---

### 1.1 Backend Work (C# / ASP.NET Web API)

#### Model & Database
- Added `Password` (`NVARCHAR(100) NULL`) and `Token` (`NVARCHAR(100) NULL`) columns to the
  `Students` table via idempotent migration SQL in `ApplicationDbContext.Initialize()`
- Kept `TeacherId` as `int` (non-nullable) — every student is always created by a teacher
- Maintained `HasRequired` foreign key relationship between `Student` and `Teacher`

#### DTOs (`TrackMyGradeAPI/DTOs/Dtos.cs`)
- Added `Password` property to `StudentDtoBase` — shared by `StudentCreateDto` and
  `StudentUpdateDto` so the teacher can set/reset credentials from the same form
- Added `StudentLoginDto` — `Email` + `Password` for student login requests
- Added `StudentAuthResponseDto` — full student profile including `Token`, returned after
  successful login and profile fetch
- Added `StudentSubmitAssessmentsDto` — three assessment scores submitted by the student

#### Service (`TrackMyGradeAPI/Services/StudentAuthService.cs`)
- Implemented `IStudentAuthService` interface with three methods:
  - `Login(StudentLoginDto)` — validates email/password, issues a fresh token, returns profile
  - `GetProfile(string token)` — looks up student by token, returns profile
  - `SubmitAssessments(string token, StudentSubmitAssessmentsDto)` — updates assessment scores

#### Controller (`TrackMyGradeAPI/Controllers/StudentAuthController.cs`)
- `POST  /api/student-auth/login` — student login
- `GET   /api/student-auth/profile` — fetch authenticated student profile (requires
  `X-StudentToken` header)
- `PUT   /api/student-auth/submit-assessments` — submit assessment scores (requires
  `X-StudentToken` header)

#### Validation (`TrackMyGradeAPI/Validators/`)
- `StudentLoginValidator` — email format + non-empty password
- `StudentCreateValidator` — inherits base rules + adds required password (6–20 chars)
- `StudentUpdateValidator` — inherits base rules + adds optional password validation
  (validates length only when a new value is provided)
- `StudentSubmitAssessmentsValidator` — each score must be between 0 and 20

#### Mapping (`TrackMyGradeAPI/Mapping/MappingProfile.cs`)
- `StudentCreateDto → Student` — `Password` now maps through (previously incorrectly ignored)
- `StudentUpdateDto → Student` — `Password` is ignored by AutoMapper; handled manually in
  the service so blank values preserve the existing password
- `Student → StudentAuthResponseDto` — full profile including token

#### Student Service Update (`TrackMyGradeAPI/Services/StudentService.cs`)
- `Update()` captures the existing password before AutoMapper runs; restores it if the
  teacher did not supply a new one

#### Dependency Injection (`TrackMyGradeAPI/Infrastructure/SimpleDependencyResolver.cs`)
- Registered `StudentAuthController`, `StudentAuthService`, `StudentLoginValidator`, and
  `StudentSubmitAssessmentsValidator`

---

### 1.2 Frontend Work (Angular 17 — `StudentApp`)

#### New Components
- **`StudentLoginComponent`** (`/student-login`)
  - Email and password fields with inline validation
  - Show/hide password toggle
  - Displays field-level and server-level errors
  - Redirects to dashboard on success
  - Informational message: *"Your teacher has created your account. Use the email and
    password they provided."*

- **`StudentDashboardComponent`** (`/student-dashboard`)
  - Fetches and displays the authenticated student's full profile on load
  - Shows student number, personal details, grade, and all three assessment scores
  - Displays computed Total, Average, Percentage, and Performance Level
  - Allows students to submit/update their own assessment scores
  - Success and error feedback on submission

#### Updated Components
- **`StudentFormComponent`** (teacher's Add/Edit student form)
  - Added `Password` field with show/hide toggle
  - On **create** — password is required (6–20 characters)
  - On **edit** — password is optional; placeholder reads *"Leave blank to keep current"*
  - Added `password-wrapper` and `.toggle-password` CSS styles

#### Services
- **`StudentAuthService`**
  - `login(data)` — `POST /api/student-auth/login`
  - `getProfile()` — `GET /api/student-auth/profile` (sends `X-StudentToken` header)
  - `submitAssessments(data)` — `PUT /api/student-auth/submit-assessments`
  - `setCurrentStudent()` — normalises API response and persists to `localStorage`
  - `logout()` — clears `localStorage` and resets the `BehaviorSubject`
  - `isAuthenticated()` — checks for a stored token
  - `getToken()` — retrieves token from `localStorage`

#### Models (`StudentApp/src/app/models/index.ts`)
- `StudentCreate` — added required `password: string`
- `StudentUpdate` — added optional `password?: string`
- Added `StudentLogin`, `StudentAuthResponse`, `StudentSubmitAssessments` interfaces

#### Routing (`app.routes.ts`)
- Added `{ path: 'student-login', component: StudentLoginComponent }`
- Added `{ path: 'student-dashboard', component: StudentDashboardComponent,
  canActivate: [studentAuthGuard] }`
- `studentAuthGuard` — redirects unauthenticated students to `/student-login`

#### Navigation Links Updated
- **Student login page** — removed "Register here" link; replaced with informational message
- **Teacher register page** — changed "Student Registration" link → "Student Login"
- **Teacher login page** — retained "Student Login" link

---

### 1.3 Cleanup — Dead Code Removed

All self-registration artefacts from a previous incorrect implementation were identified
and removed to prevent confusion and eliminate unreachable code paths.

**Backend removed:**
- `StudentRegisterDto` class
- `StudentRegisterValidator` class
- `Register(StudentRegisterDto)` from `IStudentAuthService` interface
- `Register(StudentRegisterDto)` implementation in `StudentAuthService`
- `POST /api/student-auth/register` endpoint in `StudentAuthController`
- `StudentRegisterDto → Student` AutoMapper mapping
- `IValidator<StudentRegisterDto>` DI registration in `SimpleDependencyResolver`

**Frontend removed:**
- `StudentRegisterComponent` (`.ts`, `.html`, `.css` — all three files deleted)
- `{ path: 'student-register' }` route
- `StudentRegisterComponent` import in `app.routes.ts`
- `StudentRegister` interface in `models/index.ts`
- `register()` method in frontend `StudentAuthService`
- `StudentRegister` import in frontend `StudentAuthService`

---

## 2. What Was Completed

| # | Feature / Task |
|---|----------------|
| 1 | `Student` model updated with `Password` and `Token` fields
| 2 | Idempotent DB migration for `Password` and `Token` columns |
| 3 | `TeacherId` correctly enforced as non-nullable (INT NOT NULL) |
| 4 | `StudentLoginDto`, `StudentAuthResponseDto`, `StudentSubmitAssessmentsDto` DTOs |
| 5 | `Password` added to `StudentDtoBase` (create/update) |
| 6 | `POST /api/student-auth/login` endpoint |
| 7 | `GET /api/student-auth/profile` endpoint (token-protected) |
| 8 | `PUT /api/student-auth/submit-assessments` endpoint (token-protected) |  Done |
| 9 | FluentValidation — login, create (password required), update (password optional), submit |  Done |
| 10 | AutoMapper — password flows through on create; preserved on update when blank |
| 11 | `StudentAuthService` registered in DI container |  Done |
| 12 | Angular `StudentLoginComponent` with inline validation and error handling |  Done |
| 13 | Angular `StudentDashboardComponent` with profile view and assessment submission |  Done |
| 14 | Angular `StudentAuthService` with token management and localStorage persistence |  Done |
| 15 | Password field added to teacher's student form (required on create, optional on edit) |  Done |
| 16 | `studentAuthGuard` route guard protecting student dashboard |  Done |
| 17 | All dead self-registration code removed from backend and frontend |  Done |
| 18 | Navigation links corrected across all auth pages |  Done |
| 19 | Backend compiles with **zero errors** |  Verified |
| 20 | Frontend builds with **zero errors** |  Verified |

---

## 3. Challenges Faced and How They Were Resolved

---

### Challenge 1 — Incorrect Initial Design: Student Self-Registration

**What went wrong:**
An earlier session had implemented a full student self-registration flow:
`StudentRegisterDto`, a `Register()` endpoint at `POST /api/student-auth/register`, a
`StudentRegisterComponent` in Angular, and a `/student-register` route. This was
architecturally incorrect — the system's business rule explicitly states that **teachers
create student accounts**, not students themselves.

**How it was resolved:**
The requirement was re-read and clarified. A full dependency trace was performed from
the DTO layer down through validators, services, controllers, DI registrations, AutoMapper
profiles, Angular components, routes, service methods, and model interfaces. Every
registration artefact was removed from both the backend and frontend. The password field
was instead added to the teacher's existing student creation form, making password
provisioning part of the standard teacher workflow.

---

### Challenge 2 — `TeacherId` Made Nullable, Breaking Data Integrity

**What went wrong:**
To support the (now-removed) self-registration flow, `Student.TeacherId` had been changed
to `int?` (nullable). The `HasRequired` FK relationship was changed to `HasOptional`, and
a migration script was added that dropped the foreign key constraint and executed
`ALTER TABLE dbo.Students ALTER COLUMN TeacherId INT NULL`. This undermined the core
invariant that every student belongs to a teacher.

**How it was resolved:**
- Reverted `Student.TeacherId` from `int?` back to `int`
- Reverted the EF Fluent API configuration from `HasOptional` back to `HasRequired`
- Removed the dangerous migration SQL entirely — it dropped FK constraints and altered the
  column nullability, which could corrupt referential integrity on existing databases

---

### Challenge 3 — Password Silently Discarded on Student Create

**What went wrong:**
The AutoMapper mapping for `StudentCreateDto → Student` had an explicit
`.ForMember(dest => dest.Password, opt => opt.Ignore())`. This meant that even after the
`Password` field was added to the form and sent in the request body, AutoMapper would
silently throw it away. Students would be created with `Password = null` and would be
permanently unable to log in — with no error shown to the teacher.

**How it was resolved:**
The `Password` ignore was removed from the `StudentCreateDto → Student` mapping so the
value flows directly through AutoMapper into the `Student` entity and is persisted to
the database. The ignore was kept on `StudentUpdateDto → Student` because update password
handling is intentionally manual (see Challenge 4).

---

### Challenge 4 — Existing Password Overwritten on Student Update

**What went wrong:**
When a teacher edits a student's name, grade, or assessments without intending to change
the password, the update request would send an empty `Password` field. AutoMapper would
then map that empty string onto the `Student` entity, overwriting the stored password with
`null` or `""`. The student would be locked out after any routine edit — a silent,
hard-to-diagnose bug.

**How it was resolved:**
In `StudentService.Update()`, the existing password is captured in a local variable before
AutoMapper runs. After mapping, if `request.Password` is null or empty, the captured
password is written back to the entity. This pattern makes password updates opt-in: the
teacher must explicitly type a new password to change it; leaving the field blank is safe
and leaves the student's login unaffected.

---

### Challenge 5 — Dead Code Spread Across Multiple Layers

**What went wrong:**
After identifying that self-registration was the wrong approach, simply removing the
Angular component and the API endpoint was not sufficient. Dead code remained scattered
across six distinct backend layers (DTO, validator, service interface, service
implementation, controller, DI resolver, AutoMapper profile) and five Angular layers
(model interface, service method, component files, route definition, route import).
Leaving any one piece would cause either a compile error or a misleading reference.

**How it was resolved:**
A systematic top-down audit was performed for every artefact associated with
`StudentRegister*`. The full removal chain was followed:

```
Backend:  DTO → Validator → DI Registration → Service Interface
          → Service Implementation → Controller Endpoint → AutoMapper Mapping

Frontend: Interface → Service Import → Service Method
          → Component (3 files) → Route → Route Import
```

Both projects were rebuilt after each change to verify zero errors before proceeding to
the next layer.

---

## 4. Build Verification

```
Backend  (MSBuild)  →  Build succeeded — 0 Error(s), 0 Warning(s)
Frontend (ng build) →  Build succeeded — 0 Error(s)
                        Bundle: 620.20 kB initial (warning: exceeds 512 kB budget —
                        expected; DataTables and other libraries included)
```

---

*Report generated: 2026-03-24*
