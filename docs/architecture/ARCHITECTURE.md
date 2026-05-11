# TrackMyGrade - System Architecture

## High-Level Architecture Overview

TrackMyGrade follows a **Clean Architecture with Separation of Concerns (SoC)** pattern, with the backend and frontend completely decoupled and independently runnable.

```
┌──────────────────────────────────────────────────────────────────┐
│                      Angular 18 SPA                              │
│         (Standalone Components — No NgModule Required)           │
├──────────────────────────────────────────────────────────────────┤
│  Home  | TeacherLogin | Register | StudentList | StudentForm     │
│  StudentDetail | StudentLogin | StudentDashboard                 │
│  AdminLogin | AdminDashboard                                     │
├──────────────────────────────────────────────────────────────────┤
│    Services: TeacherAuthService | StudentAuthService             │
│             AdminAuthService | StudentService | AdminApiService  │
├──────────────────────────────────────────────────────────────────┤
│              HttpClient  ·  CORS  ·  X-TeacherId Header          │
└──────────────────────────────────────────────────────────────────┘
                          HTTP/JSON
┌──────────────────────────────────────────────────────────────────┐
│              ASP.NET Web API 5.2 (.NET Framework 4.8)            │
│         (Self-hosted via OWIN/Katana on port 5000)              │
├──────────────────────────────────────────────────────────────────┤
│  Presentation/Controllers/                                       │
│  • TeachersController      (register, login, profile)           │
│  • StudentsController      (full CRUD, X-TeacherId scoped)      │
│  • StudentAuthController   (student login, profile, submit)     │
├──────────────────────────────────────────────────────────────────┤
│  Application/Services/                                           │
│  • TeacherService         • StudentService                      │
│  • StudentAuthService     • AdminService                        │
│  • AuditLogService        • AssessmentSubmissionService         │
│  • EmailService           • ExportService                       │
├──────────────────────────────────────────────────────────────────┤
│  Application/Validators/    (FluentValidation)                  │
│  • TeacherValidator  • StudentValidator  • StudentAuthValidator  │
│  • AdminValidator    • StudentAssessmentValidator               │
├──────────────────────────────────────────────────────────────────┤
│  Application/Mapping/       (AutoMapper)                        │
│  • MappingProfile                                               │
├──────────────────────────────────────────────────────────────────┤
│  Infrastructure/Data/       (EF6)                               │
│  • ApplicationDbContext → SQL Server LocalDB                    │
├──────────────────────────────────────────────────────────────────┤
│  Cross-Cutting Concerns                                          │
│  • ELMAH error logging    • Swagger (Swashbuckle 5.6)          │
│  • CORS                   • SimpleDependencyResolver (DI)       │
└──────────────────────────────────────────────────────────────────┘
```

---

## Backend Layer Breakdown

### Folder Structure

```
TrackMyGradeAPI/
├── Application/
│   ├── DTOs/           # Data Transfer Objects
│   ├── Mapping/        # AutoMapper profile
│   ├── Services/       # Business logic interfaces + implementations
│   └── Validators/     # FluentValidation validators
├── Handlers/           # ELMAH exception handler & logger
├── Infrastructure/
│   ├── Data/           # ApplicationDbContext (EF6)
│   └── SwaggerConfig.cs
├── Logging/            # ErrorLoggingConfig (ELMAH)
├── Models/             # Database entities: Teacher, Student
├── Presentation/
│   └── Controllers/    # TeachersController, StudentsController, StudentAuthController
├── Program.cs          # OWIN self-host entry point
├── Startup.cs          # OWIN middleware pipeline
└── WebApiConfig.cs     # CORS, routing, JSON config
```

---

### 1. Presentation Layer — Controllers

#### TeachersController (`/api/teachers`)
```
POST  /api/teachers/register   → Register(TeacherRegisterDto)   → TeacherResponseDto
POST  /api/teachers/login      → Login(TeacherLoginDto)          → TeacherResponseDto
GET   /api/teachers/{id}       → GetById(id)                     → TeacherResponseDto
```

#### StudentsController (`/api/students`)
All endpoints read the `X-TeacherId` request header for teacher-scoped isolation.
```
GET    /api/students           → GetAll()        → IEnumerable<StudentResponseDto>
GET    /api/students/{id}      → GetById(id)     → StudentResponseDto
POST   /api/students           → Create(dto)     → StudentResponseDto  (201 Created)
PUT    /api/students/{id}      → Update(id, dto) → StudentResponseDto
DELETE /api/students/{id}      → Delete(id)      → 200 OK
```

#### StudentAuthController (`/api/student-auth`)
Student self-service endpoints. Profile and submit-assessments read the `X-StudentToken` header.
```
POST  /api/student-auth/login                  → Login(dto)                 → StudentAuthResponseDto
GET   /api/student-auth/profile                → GetProfile()               → StudentAuthResponseDto
PUT   /api/student-auth/submit-assessments     → SubmitAssessments(dto)     → StudentAuthResponseDto
```

---

### 2. Application Layer — DTOs

#### Request DTOs (Input)
| DTO | Fields |
|-----|--------|
| `TeacherRegisterDto` | FirstName, LastName, Email, Phone, Subject, Password |
| `TeacherLoginDto` | Email, Password |
| `StudentDtoBase` (abstract) | FirstName, LastName, Email, Phone, OmangOrPassport, Grade, Assessment1-3, Password |
| `StudentCreateDto` | extends `StudentDtoBase` |
| `StudentUpdateDto` | extends `StudentDtoBase` |
| `StudentLoginDto` | Email, Password |
| `StudentSubmitAssessmentsDto` | Assessment1, Assessment2, Assessment3 |
| `AuditLogDto` | Id, EntityType, EntityName, EntityId, Action, Timestamp, Details |

#### Response DTOs (Output)
| DTO | Fields |
|-----|--------|
| `TeacherResponseDto` | Id, FirstName, LastName, Email, Phone, Subject, Token |
| `StudentResponseDto` | Id, StudentNumber, FirstName, LastName, Email, Phone, OmangOrPassport, Grade, Assessment1-3, Total, Average, Percentage, PerformanceLevel |
| `StudentAuthResponseDto` | All fields in `StudentResponseDto` + Token |

---

### 3. Application Layer — Services

| Service | Key Methods |
|---------|-------------|
| `TeacherService` | `Register()`, `Login()`, `GetById()` |
| `StudentService` | `GetAllByTeacher()`, `GetById()`, `Create()`, `Update()`, `Delete()` |
| `StudentAuthService` | `Login()`, `GetProfile()`, `SubmitAssessments()` |
| `AdminService` | Admin credential check and management |
| `AuditLogService` | Records create/update/delete events |
| `AssessmentSubmissionService` | Handles student self-assessment submission |
| `EmailService` | (Planned) email notifications |
| `ExportService` | (Planned) data export |

---

### 4. Application Layer — Validators (FluentValidation)

#### TeacherValidator
- `FirstName` / `LastName`: Required, 2–50 chars, letters only
- `Email`: Required, valid format
- `Phone`: Required, exactly 8 digits
- `Subject`: Required, max 100 chars
- `Password`: Required, 6–20 chars

#### StudentValidator
- All fields from `TeacherValidator` base rules plus:
- `OmangOrPassport`: Required, exactly 9 alphanumeric characters (`^[a-zA-Z0-9]{9}$`)
- `Grade`: Required, 1–12
- `Assessment1/2/3`: Required, 0–20

#### StudentAuthValidator
- `Email`: Required, valid format
- `Password`: Required, min 6 chars

---

### 5. Data Access Layer (EF6 + SQL Server LocalDB)

#### ApplicationDbContext
```csharp
DbSet<Teacher>  Teachers
DbSet<Student>  Students
// Additional DbSets: Admin, AuditLog (via AdminService / AuditLogService)
```

- Database: `(localdb)\MSSQLLocalDB` — `TrackMyGrade` database
- Schema initializes via `ApplicationDbContext.Initialize()` called from `Startup.cs`
- EF6 migrations applied via Package Manager Console

#### Database Schema

**Teachers**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| FirstName | NVARCHAR(50) | NOT NULL |
| LastName | NVARCHAR(50) | NOT NULL |
| Email | NVARCHAR(255) | NOT NULL, UNIQUE |
| Phone | NVARCHAR(8) | NOT NULL |
| Subject | NVARCHAR(100) | NOT NULL |
| Password | NVARCHAR(MAX) | NOT NULL (plain text) |
| Token | NVARCHAR(MAX) | NULL |

**Students**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| StudentNumber | NVARCHAR(20) | NOT NULL, system-generated (STU-YYYY-NNNN) |
| TeacherId | INT | FK → Teachers.Id, CASCADE DELETE |
| FirstName | NVARCHAR(50) | NOT NULL |
| LastName | NVARCHAR(50) | NOT NULL |
| Email | NVARCHAR(255) | NOT NULL |
| Phone | NVARCHAR(8) | NOT NULL |
| OmangOrPassport | NVARCHAR(20) | NOT NULL |
| Grade | INT | NOT NULL (1–12) |
| Assessment1 | INT | NOT NULL (0–20) |
| Assessment2 | INT | NOT NULL (0–20) |
| Assessment3 | INT | NOT NULL (0–20) |
| Password | NVARCHAR(MAX) | NOT NULL (plain text) |
| Token | NVARCHAR(MAX) | NULL |

**Computed Properties (on `Student` entity — not stored columns)**
```csharp
int    Total            => Assessment1 + Assessment2 + Assessment3
double Average          => Total / 3.0
double Percentage       => (Total / 60.0) * 100
string PerformanceLevel => < 50% → "Needs Support" | ≤ 55% → "Satisfactory"
                          | ≤ 75% → "Good" | > 75% → "Excellent"
```

---

### 6. Cross-Cutting Concerns

#### CORS
```csharp
// WebApiConfig.cs
EnableCorsAttribute("http://localhost:4200", "*", "*")
```

#### Error Handling & Logging (ELMAH)
- `ElmahExceptionHandler` and `ElmahExceptionLogger` in `Handlers/`
- Configured in `Logging/ErrorLoggingConfig.cs`
- All controller actions log via `ErrorLoggingConfig.LogError(ex)`

#### Swagger
- Swashbuckle 5.6 configured in `Infrastructure/SwaggerConfig.cs`
- UI: `http://localhost:5000/swagger`
- JSON: `http://localhost:5000/swagger/docs/v1`
- XML doc comments loaded from `TrackMyGradeAPI.XML`

#### Dependency Injection
- `SimpleDependencyResolver` in `Infrastructure/` wires service interfaces to implementations

---

## Frontend Layer Breakdown

### Folder Structure

```
StudentApp/src/app/
├── components/
│   ├── home/                  # Landing page (HomeComponent)
│   ├── teacher-login/         # TeacherLoginComponent
│   ├── register/              # RegisterComponent
│   ├── teacher-dashboard/     # TeacherDashboardComponent
│   ├── student-list/          # StudentListComponent
│   ├── student-form/          # StudentFormComponent (create + edit)
│   ├── student-detail/        # StudentDetailComponent
│   ├── student-login/         # StudentLoginComponent
│   ├── student-dashboard/     # StudentDashboardComponent
│   ├── admin-login/           # AdminLoginComponent
│   └── admin-dashboard/       # AdminDashboardComponent
├── services/
│   ├── teacher-auth.service.ts
│   ├── student-auth.service.ts
│   ├── admin-auth.service.ts  (admin-api.service.ts)
│   ├── student.service.ts
│   ├── error.util.ts
│   └── index.ts
├── models/
│   └── index.ts               # TypeScript interfaces
├── app.routes.ts              # All routes + guards
└── app.component.ts/html/css  # Root shell with navbar
```

---

### Angular Components

#### HomeComponent (`/`)
- Public landing page with animated gradient blobs background
- Three role cards: Student Access → `/student-login`, Teacher Portal → `/login`, Administration → `/admin`

#### TeacherLoginComponent (`/login`)
- Email + password login form
- Stores token + teacher in `localStorage` via `TeacherAuthService`
- On success: redirects to `/list`

#### RegisterComponent (`/register`)
- Multi-field teacher registration form
- Password and confirm-password fields
- Redirects to `/login` on success

#### StudentListComponent (`/list`) — Teacher guard
- DataTables-powered table of all students
- Columns: StudentNumber, Full Name, Email, Grade, Score, Performance badge
- View, Edit, Delete actions

#### StudentFormComponent (`/create`, `/edit/:id`) — Teacher guard
- Shared create/edit mode component
- Real-time score calculation preview (Total, Average, Percentage, Level)
- OmangOrPassport, grade, and assessment validation

#### StudentDetailComponent (`/detail/:id`) — Teacher guard
- Full student profile with all fields and computed metrics
- Performance badge and progress display
- Edit and Delete shortcuts

#### StudentLoginComponent (`/student-login`)
- Email + password login for students
- Stores token + student in `localStorage` via `StudentAuthService`

#### StudentDashboardComponent (`/student-dashboard`) — Student guard
- Personal profile display (name, email, grade, student number)
- Assessment scores (1, 2, 3) and calculated metrics
- Performance level badge

#### AdminLoginComponent (`/admin`)
- Admin email + password login
- Stores admin session via `AdminAuthService`

#### AdminDashboardComponent (`/admin-dashboard`) — Admin guard
- Tabbed interface: Teachers | Students | Audit Logs
- Reads from `AdminApiService`

---

### Angular Services

| Service | Responsibilities |
|---------|-----------------|
| `TeacherAuthService` | register, login, logout, `isAuthenticated()`, `currentTeacher$` BehaviorSubject |
| `StudentAuthService` | login, logout, `isAuthenticated()`, `currentStudent$` BehaviorSubject |
| `AdminAuthService` | admin login, logout, `isAuthenticated()`, `currentAdmin$` BehaviorSubject |
| `StudentService` | CRUD calls to `/api/students` with `X-TeacherId` header |
| `AdminApiService` | reads teachers, students, audit logs via admin endpoints |
| `error.util.ts` | shared error parsing utility |

---

### Route Guards

| Guard | Type | Redirects to |
|-------|------|-------------|
| `authGuard` | `CanActivateFn` | `/login` if teacher not authenticated |
| `studentAuthGuard` | `CanActivateFn` | `/student-login` if student not authenticated |
| Admin guard | inline in routes | `/admin` if admin not authenticated |

---

### Angular Models (TypeScript Interfaces — `models/index.ts`)

```typescript
Admin              { id, firstName, lastName, email, phone, token }
Teacher            { id, firstName, lastName, email, phone, subject, token }
Student            { id, studentNumber, firstName, lastName, email, phone, omangOrPassport, grade,
                     assessment1, assessment2, assessment3, total, average, percentage, performanceLevel }
AdminLogin         { email, password }
TeacherRegister    { firstName, lastName, email, phone, subject, password }
TeacherLogin       { email, password }
StudentCreate      { firstName, lastName, email, phone, omangOrPassport, grade,
                     assessment1, assessment2, assessment3, password }
StudentUpdate      { firstName, lastName, email, phone, omangOrPassport, grade,
                     assessment1, assessment2, assessment3, password? }
StudentLogin       { email, password }
StudentAuthResponse{ id, studentNumber, firstName, lastName, email, phone, omangOrPassport,
                     grade, assessment1-3, total, average, percentage, performanceLevel, token }
StudentSubmitAssessments { assessment1, assessment2, assessment3 }
AuditLogDto        { id, entityType, entityName, entityId, action, timestamp, details }
TabType            'teachers' | 'students' | 'auditLogs'
```

---

## Data Flow Examples

### Example 1: Teacher Registration & Login
```
Teacher navigates to /register
  ↓
RegisterComponent validates form (Angular template-driven)
  ↓
TeacherAuthService.register() → POST /api/teachers/register
  ↓
TeachersController → TeacherService.Register()
  ↓
TeacherValidator checks all fields
  ↓
Student entity saved to SQL Server LocalDB
  ↓
Returns TeacherResponseDto (200 OK)
  ↓
Angular redirects to /login
  ↓
TeacherAuthService.login() → POST /api/teachers/login
  ↓
Token + profile stored in localStorage
  ↓
Navbar shows teacher name; guarded routes unlock
```

### Example 2: Teacher Creates a Student
```
Teacher navigates to /create
  ↓
StudentFormComponent validates all fields + calculates scores in real time
  ↓
StudentService.createStudent() → POST /api/students  (X-TeacherId header)
  ↓
StudentsController → StudentService.Create()
  ↓
StudentValidator checks all fields
  ↓
StudentNumber generated (STU-YYYY-NNNN)
  ↓
Student saved to DB; calculated properties computed from entity
  ↓
Returns StudentResponseDto (201 Created)
  ↓
Angular redirects to /list
```

### Example 3: Student Login & Dashboard
```
Student navigates to /student-login
  ↓
StudentLoginComponent → StudentAuthService.login()
  ↓
POST /api/student-auth/login  (email + password)
  ↓
StudentAuthController → StudentAuthService.Login()
  ↓
Returns StudentAuthResponseDto with token
  ↓
Token + student stored in localStorage
  ↓
Angular navigates to /student-dashboard
  ↓
StudentDashboardComponent displays profile, scores, performance level
```

---

## Security Considerations

### Current Implementation (Development)
```
Basic auth (email/password comparison — plain text)
Simple GUID tokens (no expiry)
Teacher isolation via X-TeacherId header (not verified)
Student isolation via X-StudentToken header (not verified)
No HTTPS
No rate limiting
```

### Production Recommendations
```
Password hashing: bcrypt or Argon2
Signed JWT with expiry and refresh tokens
HTTPS/TLS
Role-based access control (RBAC) enforced server-side
Rate limiting and DDoS protection
Audit logging (already partially implemented)
SQL injection prevention (EF6 parameterized queries — already covered)
CORS whitelist (specific origins only)
```

---

## Performance Considerations

- SQL Server LocalDB: persistent, low-latency for development datasets
- EF6 change tracking optimized per-request via scoped DbContext
- Angular: standalone components reduce bundle overhead
- No additional caching layer needed for development scale

### Response Time Target
- API responses: < 500 ms (development)
- Client rendering: < 100 ms
- Total page load: < 2 seconds

---

## Deployment Architecture

### Development
```
Backend:  OWIN self-hosted console app (port 5000)
Frontend: Angular dev server (port 4200)
Database: SQL Server LocalDB  ((localdb)\MSSQLLocalDB)
```

### Production (Recommended)
```
Backend:  IIS with Application Pool (.NET Framework 4.8)
          SQL Server (full instance)
          HTTPS enabled
Frontend: CDN or static host (minified Angular build)
          HTTPS with valid certificate
```

---

## Summary

TrackMyGrade architecture ensures:
- **Separation of Concerns**: Clean five-layer backend + decoupled Angular SPA
- **Multi-role access**: Admin, Teacher, and Student roles with independent auth flows
- **Loose Coupling**: Frontend and backend communicate only via REST/JSON
- **Auditability**: AuditLogService tracks all data mutations
- **Testability**: Each layer (controller, service, validator) is independently testable
- **Maintainability**: Clear folder structure and naming conventions throughout
