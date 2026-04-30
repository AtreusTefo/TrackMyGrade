# TrackMyGrade - Complete Project Deliverables

## Project Overview

TrackMyGrade is a fully functional, multi-role full-stack web application for managing student assessments. It supports three user roles — **Admin**, **Teacher**, and **Student** — built on clean architecture principles with a layered ASP.NET backend and a standalone Angular 18 frontend.

**Status**: Sprint 4 In Progress — Core system fully functional

---

## Backend Deliverables (ASP.NET Framework 4.8)

### Technology
- **Framework**: .NET Framework 4.8 with ASP.NET Web API 5.2
- **Database**: SQL Server LocalDB (`(localdb)\MSSQLLocalDB` — `TrackMyGrade` database)
- **ORM**: Entity Framework 6.4 (Code-First)
- **Hosting**: OWIN/Katana self-hosted on port 5000

### Project Structure
```
TrackMyGradeAPI/
├── Application/
│   ├── DTOs/         # Request/Response Data Transfer Objects
│   ├── Mapping/      # AutoMapper profile
│   ├── Services/     # Business logic layer
│   └── Validators/   # FluentValidation validators
├── Handlers/         # ELMAH exception handler & logger
├── Infrastructure/
│   ├── Data/         # ApplicationDbContext (EF6)
│   └── SwaggerConfig.cs
├── Logging/          # ErrorLoggingConfig
├── Models/           # Teacher, Student entities
├── Presentation/
│   └── Controllers/  # TeachersController, StudentsController, StudentAuthController
├── Program.cs        # OWIN self-host entry point
├── Startup.cs        # Middleware pipeline
└── WebApiConfig.cs   # CORS, routing, camelCase JSON
```

---

### Data Access Layer

- **File**: `Infrastructure/Data/ApplicationDbContext.cs`
- EF6 Code-First context with `DbSet<Teacher>` and `DbSet<Student>`
- One-to-Many relationship: Teacher → Students (cascade delete)
- Auto-initializes schema on startup via `ApplicationDbContext.Initialize()`
- Database: SQL Server LocalDB — persists between restarts

---

### Models (Entities)

- **File**: `Models/Student.cs`

| Entity | Fields |
|--------|--------|
| `Teacher` | Id, FirstName, LastName, Email, Phone, Subject, Password, Token |
| `Student` | Id, StudentNumber, TeacherId, FirstName, LastName, Email, Phone, OmangOrPassport, Grade, Assessment1-3, Password, Token |

**Computed Properties (Student entity — not stored in DB)**
- `Total` = Assessment1 + Assessment2 + Assessment3 (max 60)
- `Average` = Total / 3.0
- `Percentage` = (Total / 60.0) × 100
- `PerformanceLevel` — derived from Percentage band

---

### Data Transfer Objects (DTOs)

- **Files**: `Application/DTOs/StudentDto.cs`, `TeacherDto.cs`, `AdminDto.cs`, `AuditLogDto.cs`, etc.

| DTO | Type | Purpose |
|-----|------|---------|
| `StudentDtoBase` | Abstract | Shared fields for create/update (incl. OmangOrPassport, Password) |
| `StudentCreateDto` | Request | Create a new student |
| `StudentUpdateDto` | Request | Update an existing student |
| `StudentResponseDto` | Response | Full student record with computed metrics |
| `StudentLoginDto` | Request | Student self-service login |
| `StudentSubmitAssessmentsDto` | Request | Student submits own scores |
| `StudentAuthResponseDto` | Response | Student profile + Token |
| `TeacherRegisterDto` | Request | Teacher registration |
| `TeacherLoginDto` | Request | Teacher login |
| `TeacherResponseDto` | Response | Teacher profile + Token |
| `AuditLogDto` | Response | Audit log entry |

---

### Validation Layer

- **Files**: `Application/Validators/`

| Validator | Rules |
|-----------|-------|
| `TeacherValidator` (register + login) | FirstName/LastName (2–50 chars), Email, Phone (8 digits), Subject (max 100), Password (6–20) |
| `StudentValidator` (create + update) | All teacher fields + OmangOrPassport (9 alphanumeric), Grade (1–12), Assessment1-3 (0–20) |
| `StudentAuthValidator` | Email (required, valid format), Password (min 6 chars) |
| `StudentAssessmentValidator` | Name (required), MaxScore (> 0), Score (0 ≤ score ≤ MaxScore) |
| `AdminValidator` | Admin credential validation |

---

### Services (Business Logic Layer)

- **Files**: `Application/Services/`

| Service | Key Methods |
|---------|-------------|
| `TeacherService` | `Register()`, `Login()`, `GetById()` |
| `StudentService` | `GetAllByTeacher()`, `GetById()`, `Create()`, `Update()`, `Delete()` |
| `StudentAuthService` | `Login()`, `GetProfile()`, `SubmitAssessments()` |
| `AdminService` | Admin login and oversight |
| `AuditLogService` | Records entity-level changes with timestamps |
| `AssessmentSubmissionService` | Student self-assessment handling |
| `EmailService` | (Planned) email notifications |
| `ExportService` | (Planned) data export |

---

### API Controllers (Presentation Layer)

- **Files**: `Presentation/Controllers/`

#### TeachersController (`/api/teachers`)
```
POST  /api/teachers/register   → 200 OK + TeacherResponseDto
POST  /api/teachers/login      → 200 OK + TeacherResponseDto
GET   /api/teachers/{id}       → 200 OK + TeacherResponseDto
```

#### StudentsController (`/api/students`) — requires `X-TeacherId` header
```
GET    /api/students           → 200 OK + StudentResponseDto[]
GET    /api/students/{id}      → 200 OK + StudentResponseDto
POST   /api/students           → 201 Created + StudentResponseDto
PUT    /api/students/{id}      → 200 OK + StudentResponseDto
DELETE /api/students/{id}      → 200 OK + { message }
```

#### StudentAuthController (`/api/student-auth`)
```
POST  /api/student-auth/login                → 200 OK + StudentAuthResponseDto
GET   /api/student-auth/profile              → 200 OK + StudentAuthResponseDto  (X-StudentToken)
PUT   /api/student-auth/submit-assessments   → 200 OK + StudentAuthResponseDto  (X-StudentToken)
```

---

### Mapping Configuration

- **File**: `Application/Mapping/MappingProfile.cs`
- AutoMapper profile maps:
  - `TeacherRegisterDto` → `Teacher`
  - `Teacher` → `TeacherResponseDto`
  - `StudentCreateDto/StudentUpdateDto` → `Student`
  - `Student` → `StudentResponseDto` (includes computed properties)
  - `Student` → `StudentAuthResponseDto`

---

### CORS Configuration

- **File**: `WebApiConfig.cs`
- Allows requests from `http://localhost:4200` (Angular dev server)
- Permits all HTTP methods and headers
- JSON serialized as camelCase via `CamelCasePropertyNamesContractResolver`

---

### Error Logging (ELMAH)

- `ElmahExceptionHandler` and `ElmahExceptionLogger` in `Handlers/`
- Configured via `Logging/ErrorLoggingConfig.cs`
- All controller catch blocks log via `ErrorLoggingConfig.LogError(ex)`

---

### API Documentation (Swagger)

- **File**: `Infrastructure/SwaggerConfig.cs`
- Swashbuckle 5.6 — registered from `WebApiConfig.cs`
- UI: `http://localhost:5000/swagger`
- JSON: `http://localhost:5000/swagger/docs/v1`
- XML doc comments loaded from `TrackMyGradeAPI.XML`

---

## Frontend Deliverables (Angular 18)

### Technology
- **Framework**: Angular 18 (Standalone Components — no NgModule)
- **Language**: TypeScript 5.2 (`moduleResolution: "bundler"`)
- **Build Tool**: Angular CLI 18
- **Dev Server**: `http://localhost:4200`
- **Production Output**: `dist/StudentApp/browser/`

---

### Configuration Files

| File | Purpose |
|------|---------|
| `angular.json` | Angular CLI build, serve, test config |
| `tsconfig.json` | TypeScript compiler options (`moduleResolution: "bundler"`) |
| `tsconfig.app.json` | App-specific TypeScript config |
| `package.json` | npm dependencies and scripts |

---

### Routes & Guards

- **File**: `src/app/app.routes.ts`

| Route | Component | Guard |
|-------|-----------|-------|
| `/` | `HomeComponent` | Public |
| `/login` | `TeacherLoginComponent` | Public |
| `/register` | `RegisterComponent` | Public |
| `/list` | `StudentListComponent` | `authGuard` (teacher) |
| `/create` | `StudentFormComponent` | `authGuard` (teacher) |
| `/edit/:id` | `StudentFormComponent` | `authGuard` (teacher) |
| `/detail/:id` | `StudentDetailComponent` | `authGuard` (teacher) |
| `/student-login` | `StudentLoginComponent` | Public |
| `/student-dashboard` | `StudentDashboardComponent` | `studentAuthGuard` |
| `/admin` | `AdminLoginComponent` | Public |
| `/admin-dashboard` | `AdminDashboardComponent` | Admin check |
| `**` | — | Redirects to `/` |

---

### Models (TypeScript Interfaces)

- **File**: `src/app/models/index.ts`

```typescript
Admin                  { id, firstName, lastName, email, phone, token }
Teacher                { id, firstName, lastName, email, phone, subject, token }
Student                { id, studentNumber, firstName, lastName, email, phone,
                         omangOrPassport, grade, assessment1-3, total, average,
                         percentage, performanceLevel }
AdminLogin             { email, password }
TeacherRegister        { firstName, lastName, email, phone, subject, password }
TeacherLogin           { email, password }
StudentCreate          { firstName, lastName, email, phone, omangOrPassport,
                         grade, assessment1-3, password }
StudentUpdate          { firstName, lastName, email, phone, omangOrPassport,
                         grade, assessment1-3, password? }
StudentLogin           { email, password }
StudentAuthResponse    { id, studentNumber, firstName, lastName, email, phone,
                         omangOrPassport, grade, assessment1-3, total, average,
                         percentage, performanceLevel, token }
StudentSubmitAssessments { assessment1, assessment2, assessment3 }
AuditLogDto            { id, entityType, entityName, entityId, action, timestamp, details }
TabType                'teachers' | 'students' | 'auditLogs'
```

---

### Services

- **Files**: `src/app/services/`

| Service | Responsibilities |
|---------|-----------------|
| `TeacherAuthService` | `register()`, `login()`, `logout()`, `isAuthenticated()`, `currentTeacher$` |
| `StudentAuthService` | `login()`, `logout()`, `isAuthenticated()`, `currentStudent$` |
| `AdminAuthService` / `AdminApiService` | Admin login, teacher/student/audit log API calls |
| `StudentService` | CRUD calls to `/api/students` with `X-TeacherId` header |
| `error.util.ts` | Shared error parsing utility for HTTP errors |

---

### Components (Standalone)

#### HomeComponent — `/`
- Animated gradient-blob landing page
- Three role cards: Student Access, Teacher Portal, Administration
- Links to `/student-login`, `/login`, `/admin`

#### TeacherLoginComponent — `/login`
- Email + password form with inline validation
- On success: stores session via `TeacherAuthService`, redirects to `/list`

#### RegisterComponent — `/register`
- Full teacher registration form (name, email, phone, subject, password, confirm-password)
- Inline validation matching backend FluentValidation rules
- On success: redirects to `/login`

#### StudentListComponent — `/list` (Teacher guard)
- DataTables-powered table: StudentNumber, Full Name, Email, Grade, Score, Performance badge
- Search, sort, pagination
- View / Edit / Delete actions with confirmation dialog

#### StudentFormComponent — `/create`, `/edit/:id` (Teacher guard)
- Shared create + edit mode
- OmangOrPassport, Grade, Assessment1-3, Password fields
- Real-time calculation preview (Total, Average, Percentage, Level badge)
- Auto-generated `StudentNumber` on create

#### StudentDetailComponent — `/detail/:id` (Teacher guard)
- Full student profile with all fields and computed metrics
- Performance badge and score display
- Edit and Delete shortcuts

#### StudentLoginComponent — `/student-login`
- Email + password login for students
- Stores session via `StudentAuthService`, redirects to `/student-dashboard`

#### StudentDashboardComponent — `/student-dashboard` (Student guard)
- Personal profile (name, email, phone, grade, student number)
- Assessment scores (Assessment 1, 2, 3)
- Calculated metrics: Total, Average, Percentage, Performance level

#### AdminLoginComponent — `/admin`
- Admin email + password login
- Stores session via `AdminAuthService`, redirects to `/admin-dashboard`

#### AdminDashboardComponent — `/admin-dashboard` (Admin)
- Tabbed interface: Teachers | Students | Audit Logs
- Powered by `AdminApiService`

---

### Root Shell

- **Files**: `app.component.ts/html/css`
- Navigation bar showing logged-in user info (name) and logout button
- Role-aware: shows correct nav for Admin / Teacher / Student
- Authenticated admin navbar links to `/admin-dashboard`
- Authenticated teacher navbar links to `/list`
- Authenticated student navbar links to `/student-dashboard`
- `<router-outlet>` renders active route component

---

## Database Schema

### Teachers Table
```sql
CREATE TABLE Teachers (
  Id          INT           PRIMARY KEY IDENTITY,
  FirstName   NVARCHAR(50)  NOT NULL,
  LastName    NVARCHAR(50)  NOT NULL,
  Email       NVARCHAR(255) NOT NULL UNIQUE,
  Phone       NVARCHAR(8)   NOT NULL,
  Subject     NVARCHAR(100) NOT NULL,
  Password    NVARCHAR(MAX) NOT NULL,
  Token       NVARCHAR(MAX) NULL
)
```

### Students Table
```sql
CREATE TABLE Students (
  Id               INT           PRIMARY KEY IDENTITY,
  StudentNumber    NVARCHAR(20)  NOT NULL,          -- STU-YYYY-NNNN
  TeacherId        INT           NOT NULL REFERENCES Teachers(Id) ON DELETE CASCADE,
  FirstName        NVARCHAR(50)  NOT NULL,
  LastName         NVARCHAR(50)  NOT NULL,
  Email            NVARCHAR(255) NOT NULL,
  Phone            NVARCHAR(8)   NOT NULL,
  OmangOrPassport  NVARCHAR(20)  NOT NULL,
  Grade            INT           NOT NULL,          -- 1-12
  Assessment1      INT           NOT NULL,          -- 0-20
  Assessment2      INT           NOT NULL,          -- 0-20
  Assessment3      INT           NOT NULL,          -- 0-20
  Password         NVARCHAR(MAX) NOT NULL,
  Token            NVARCHAR(MAX) NULL
)
```

*Computed fields (Total, Average, Percentage, PerformanceLevel) are derived from entity computed properties — not stored columns.*

---

## Calculations Implementation

```
Total       = Assessment1 + Assessment2 + Assessment3   (max 60)
Average     = Total / 3
Percentage  = (Total / 60) × 100

Performance Level:
  > 75%     → Excellent
  56–75%    → Good
  50–55%    → Satisfactory
  < 50%     → Needs Support
```

Implemented in:
- **Backend**: `Student` entity computed properties (`Student.cs`)
- **Frontend**: `StudentFormComponent` real-time preview display

---

## Features Implemented

### Authentication & Session
- ✅ Teacher register + login (GUID token, `localStorage`)
- ✅ Student login (email + password, GUID token, `localStorage`)
- ✅ Admin login (separate auth flow)
- ✅ Route guards: `authGuard` (teacher), `studentAuthGuard` (student)
- ✅ Navbar logout for all three roles

### Student Management (Teacher)
- ✅ Create student with OmangOrPassport, Grade, 3 assessments, initial password
- ✅ Auto-generated `StudentNumber` (format: `STU-YYYY-NNNN`)
- ✅ List all students (DataTables: sort, search, paginate)
- ✅ View detailed student profile
- ✅ Edit student record
- ✅ Delete student with confirmation dialog
- ✅ Teacher-scoped isolation via `X-TeacherId` header

### Student Self-Service Portal
- ✅ Student login at `/student-login`
- ✅ Personal dashboard at `/student-dashboard`
- ✅ View own profile, scores, and performance metrics
- ✅ Student score submission via API (`PUT /api/student-auth/submit-assessments`)

### Admin Dashboard
- ✅ Admin login at `/admin`
- ✅ Tabbed dashboard: Teachers | Students | Audit Logs
- ✅ Audit log records entity-level mutations with timestamps

### Calculations
- ✅ Automatic Total, Average, Percentage calculation
- ✅ Performance level classification (four bands)
- ✅ Real-time calculation preview on student form

### Validation
- ✅ Client-side Angular template-driven validators
- ✅ Server-side FluentValidation on all DTOs
- ✅ Inline error messages on all forms
- ✅ OmangOrPassport format validation (`^[a-zA-Z0-9]{9}$`)

### Home Page
- ✅ Animated landing page at `/` (gradient blobs background)
- ✅ Three role access cards (Student, Teacher, Admin)

### API & Tooling
- ✅ Swagger UI at `http://localhost:5000/swagger`
- ✅ ELMAH error logging
- ✅ Postman collection + environment files
- ✅ `start-api.ps1` helper script

---

## File Count Summary

| Category | Count |
|----------|-------|
| Backend C# Files | ~20 |
| Frontend TypeScript Files | ~30 |
| Frontend HTML Templates | 11 |
| Frontend CSS Files | 11 |
| Configuration Files | 8 |
| Documentation Files | 6 |

---

## How to Start

### Quick Start (5 minutes)
1. Open `TrackMyGradeAPI.csproj` in Visual Studio → F5 (API on port 5000)
2. `cd StudentApp && npm install && npm start` (Angular on port 4200)
3. Navigate to `http://localhost:4200`

### First-Time Usage
1. Go to `/register` → create a teacher account
2. Log in at `/login`
3. Add students at `/create`
4. Students log in at `/student-login`
5. Admin logs in at `/admin`

---

## Security Reminders

| Item | Current State | Production Target |
|------|--------------|-------------------|
| Passwords | Plain text ⚠️ | bcrypt / Argon2 |
| Tokens | GUID (no expiry) ⚠️ | Signed JWT with expiry |
| Teacher isolation | `X-TeacherId` header ⚠️ | Verified from signed token |
| HTTPS | Not enforced ⚠️ | TLS/SSL required |
| Authorization | Login-state only ⚠️ | Server-side RBAC |

---

**Last Updated**: April 2026
**Project Status**: Sprint 4 In Progress
**Version**: 1.3.0
