# TrackMyGrade - Setup and Build Instructions

## Overview
TrackMyGrade is a full-stack web application built with ASP.NET Framework 4.8 (backend) and Angular 18 (frontend). It supports three user roles — **Admin**, **Teacher**, and **Student** — enabling teachers to manage student records and assessments with automatic performance calculations, students to log in and view their own results, and admins to oversee the entire system.

---

## Project Structure

```
TrackMyGrade/
├── TrackMyGradeAPI/                  # Backend ASP.NET Framework API
│   ├── Application/
│   │   ├── DTOs/                     # Data Transfer Objects (StudentDto, TeacherDto, etc.)
│   │   ├── Mapping/                  # AutoMapper profile
│   │   ├── Services/                 # Business logic (TeacherService, StudentService, etc.)
│   │   └── Validators/               # FluentValidation validators
│   ├── Handlers/                     # ELMAH exception handler & logger
│   ├── Infrastructure/
│   │   ├── Data/                     # ApplicationDbContext (EF6, SQL Server LocalDB)
│   │   └── SwaggerConfig.cs          # Swashbuckle setup
│   ├── Logging/                      # ErrorLoggingConfig (ELMAH)
│   ├── Models/                       # Database entities: Teacher, Student
│   ├── Presentation/
│   │   └── Controllers/              # TeachersController, StudentsController, StudentAuthController
│   ├── Program.cs                    # Console entry point (OWIN self-host)
│   ├── Startup.cs                    # OWIN middleware pipeline
│   ├── WebApiConfig.cs               # CORS, routing, JSON camelCase config
│   ├── web.config                    # OWIN host configuration
│   └── Global.asax.cs                # Application lifecycle hooks
│
├── StudentApp/                       # Frontend Angular 18 SPA
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   │   ├── home/             # Landing page (HomeComponent)
│   │   │   │   ├── login/            # Generic login component
│   │   │   │   ├── register/         # RegisterComponent
│   │   │   │   ├── teacher-dashboard/# TeacherDashboardComponent
│   │   │   │   ├── student-list/     # StudentListComponent
│   │   │   │   ├── student-form/     # StudentFormComponent (create + edit)
│   │   │   │   ├── student-detail/   # StudentDetailComponent
│   │   │   │   ├── student-login/    # StudentLoginComponent
│   │   │   │   ├── student-dashboard/# StudentDashboardComponent
│   │   │   │   ├── admin-dashboard/  # AdminDashboardComponent
│   │   │   │   └── activate/         # Route guards (CanActivateFn implementations)
│   │   │   ├── services/             # TeacherAuthService, StudentAuthService, AdminAuthService, etc.
│   │   │   ├── models/               # TypeScript interfaces (index.ts)
│   │   │   ├── app.routes.ts         # Routes + CanActivateFn guards
│   │   │   └── app.component.ts/html # Root shell + navbar
│   │   ├── main.ts                   # Angular bootstrap
│   │   ├── index.html                # HTML entry point
│   │   └── styles.css                # Global styles
│   ├── angular.json
│   ├── tsconfig.json                 # moduleResolution: "bundler"
│   └── package.json
│
├── .github/                          # GitHub configuration
│   ├── copilot-instructions.md      # AI development guidelines
│   └── workflows/                   # CI/CD workflows (if present)
├── docs/                             # Project documentation
│   ├── architecture/ARCHITECTURE.md
│   ├── implementation/               # Implementation guides and reports
│   ├── guides/                       # Quick start and setup guides
│   ├── daily-reports/               # Development progress reports
│   ├── api-postman/                 # Postman integration guides
│   ├── project/                     # Project requirements and deliverables
│   │   ├── PROJECT_REQUIREMENTS.md
│   │   ├── DELIVERABLES.md
│   │   └── AGILE_HIERACHY.md
│   ├── error-fixes/FIX_ERRORS.md
│   └── DOCUMENTATION_INDEX.md       # Master documentation index
├── TrackMyGradeAPI.postman_collection.json
├── TrackMyGradeAPI.postman_environment.json
└── README.md                         # This file
```

---

## Backend Setup (ASP.NET Framework)

### Prerequisites
- Visual Studio 2019 or later (with .NET Framework 4.8 SDK)
- SQL Server LocalDB (included with Visual Studio)
- NuGet Package Manager

### NuGet Dependencies
| Package | Version | Purpose |
|---------|---------|---------|
| `EntityFramework` | 6.4.4 | ORM — SQL Server LocalDB |
| `AutoMapper` | 10.1.1 | Object-to-object mapping |
| `FluentValidation` | 11.8.1 | Server-side model validation |
| `Microsoft.AspNet.WebApi` | 5.2.9 | REST API framework |
| `Microsoft.AspNet.WebApi.Cors` | 5.2.9 | CORS support |
| `Microsoft.AspNet.WebApi.Owin` | 5.2.9 | OWIN integration |
| `Microsoft.Owin` | 4.2.2 | OWIN abstractions |
| `Microsoft.Owin.SelfHost` | 4.2.2 | Self-hosting |
| `ELMAH` | 1.2.2 | Error logging |
| `Swashbuckle.Core` | 5.6.0 | Swagger/OpenAPI docs |

### Build Instructions

1. **Open in Visual Studio**
   ```
   Open TrackMyGradeAPI/TrackMyGradeAPI.csproj
   ```

2. **Restore NuGet packages**
   ```
   Right-click project → Restore NuGet Packages
   OR in Package Manager Console: Update-Package
   ```

3. **Build the project**
   ```
   Build → Build Solution  (Ctrl+Shift+B)
   ```

4. **Database initialization**
   - The `TrackMyGrade` database is created automatically in SQL Server LocalDB on first run
   - `ApplicationDbContext.Initialize()` is called from `Startup.cs`
   - No external database setup required

5. **Run the API**
   ```
   Debug → Start Debugging (F5)
   OR: cd TrackMyGradeAPI && .\start-api.ps1
   ```
   - API: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`

---

## API Endpoints Reference

### Teacher Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/teachers/register` | Register a new teacher |
| POST | `/api/teachers/login` | Teacher login — returns token |
| GET | `/api/teachers/{id}` | Get teacher profile by ID |

### Student Endpoints (Teacher-scoped — requires `X-TeacherId` header)
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/students` | List all students for the teacher |
| GET | `/api/students/{id}` | Get student details |
| POST | `/api/students` | Create new student |
| PUT | `/api/students/{id}` | Update student |
| DELETE | `/api/students/{id}` | Delete student |

### Student Auth Endpoints (Student self-service)
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/student-auth/login` | Student login |
| GET | `/api/student-auth/profile` | Get own profile (`X-StudentToken` header) |
| PUT | `/api/student-auth/submit-assessments` | Submit own scores (`X-StudentToken` header) |

---

## Postman Integration

**TrackMyGrade API is fully integrated with Postman.**

### Quick Start

1. **Import Collection**: `TrackMyGradeAPI.postman_collection.json`
2. **Import Environment**: `TrackMyGradeAPI.postman_environment.json` (pre-configured for `http://localhost:5000`)
3. **Start Testing**: Run "Register Teacher" first — teacher ID is auto-saved for subsequent requests

### Newman CLI (Optional)
```powershell
npm install -g newman
.\TrackMyGradeAPI\run-postman-tests.ps1
```

---

## Frontend Setup (Angular 18)

### Prerequisites
- Node.js 18+ and npm 9+
- Angular CLI 18: `npm install -g @angular/cli`

### Installation

```bash
cd StudentApp
npm install
npm start          # Dev server at http://localhost:4200
```

### Production Build
```bash
ng build --configuration production
# Output: dist/StudentApp/browser/
```

---

## Application Routes

| URL | Component | Role Required |
|-----|-----------|--------------|
| `/` | `HomeComponent` | Public — landing page |
| `/login` | `LoginComponent` | Public — generic login (teacher/admin) |
| `/register` | `RegisterComponent` | Public |
| `/teacher-dashboard` | `TeacherDashboardComponent` | Teacher (auth guard) |
| `/student-list` | `StudentListComponent` | Teacher (auth guard) |
| `/student-create` | `StudentFormComponent` | Teacher (auth guard) |
| `/student-edit/:id` | `StudentFormComponent` | Teacher (auth guard) |
| `/student-detail/:id` | `StudentDetailComponent` | Teacher (auth guard) |
| `/student-login` | `StudentLoginComponent` | Public |
| `/student-dashboard` | `StudentDashboardComponent` | Student (student auth guard) |
| `/admin-dashboard` | `AdminDashboardComponent` | Admin (admin auth guard) |
| `**` | — | Redirects to `/` |

---

## Documentation

For detailed documentation, start with:

1. **[docs/DOCUMENTATION_INDEX.md](docs/DOCUMENTATION_INDEX.md)** — Master documentation index
2. **[docs/guides/QUICKSTART.md](docs/guides/QUICKSTART.md)** — 5-minute quick start
3. **[docs/architecture/ARCHITECTURE.md](docs/architecture/ARCHITECTURE.md)** — System architecture
4. **[docs/project/PROJECT_REQUIREMENTS.md](docs/project/PROJECT_REQUIREMENTS.md)** — Full requirements

---

### Authentication & Authorization
- **Teacher**: Register + login; session persisted in `localStorage` via `TeacherAuthService`
- **Student**: Login with email/password set by teacher; session via `StudentAuthService`
- **Admin**: Separate login via `AdminAuthService`
- **Route Guards**: Located in `app/activate/` — `CanActivateFn` style guards enforce role-based access control
  - `authGuard` — Protects teacher routes
  - `studentAuthGuard` — Protects student routes
  - `adminAuthGuard` — Protects admin routes

### Student Management
- **Create**: Form with `OmangOrPassport`, grade, three assessments, initial password
- **Auto StudentNumber**: Generated in the format `STU-YYYY-NNNN`
- **List**: DataTables table with performance badge, search, pagination
- **Edit / Delete**: With confirmation dialog

### Calculations (computed on `Student` entity — not stored)
```
Total       = Assessment1 + Assessment2 + Assessment3  (max 60)
Average     = Total / 3
Percentage  = (Total / 60) × 100
Performance = < 50% → Needs Support | 50-55% → Satisfactory
              56-75% → Good         | > 75%  → Excellent
```

### Student Self-Service Portal
- Students log in independently at `/student-login`
- Dashboard shows full profile, scores, and colour-coded performance
- Students may submit their own scores via the API

### Admin Dashboard
- Tabbed interface: Teachers | Students | Audit Logs
- Audit log records all create/update/delete actions with timestamps

---

## Configuration

### Backend CORS
```csharp
// WebApiConfig.cs
new EnableCorsAttribute("http://localhost:4200", "*", "*")
```

### JSON Serialization
Configured for camelCase output in `WebApiConfig.cs`:
```csharp
config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
    new CamelCasePropertyNamesContractResolver();
```

### Frontend API Base URLs
Defined per service in `services/`:
```typescript
private apiUrl = 'http://localhost:5000/api/...';
```
Change the base URL when deploying to production.

### TypeScript Configuration
`tsconfig.json` uses `"moduleResolution": "bundler"` — required for Angular 18 with `.NET Framework` backend. Do **not** revert to `"node"`.

---

## Running the Full Application

**Terminal 1 — Backend API**
```powershell
cd TrackMyGradeAPI
# Option 1: Start via Visual Studio (F5)
# Option 2: Using PowerShell script
.\start-api.ps1
# Option 3: Direct dotnet run (if .NET Framework configured)
```

**Terminal 2 — Frontend**
```bash
cd StudentApp
npm install    # First time only
npm start      # Dev server at http://localhost:4200
```

Navigate to `http://localhost:4200`.

---

## Data Models

### Teacher Entity
```json
{
  "id": 1,
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "smith@school.com",
  "phone": "12345678",
  "subject": "Mathematics",
  "password": "password123",
  "token": "unique-guid-token"
}
```

### Student Entity
```json
{
  "id": 1,
  "studentNumber": "STU-2026-0001",
  "teacherId": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@school.com",
  "phone": "87654321",
  "omangOrPassport": "AB1234567",
  "grade": 10,
  "assessment1": 18,
  "assessment2": 19,
  "assessment3": 17,
  "password": "student123",
  "token": "student-guid-token"
}
```

*(Calculated fields `total`, `average`, `percentage`, `performanceLevel` are computed by the entity and included in response DTOs.)*

---

## Troubleshooting

For comprehensive troubleshooting guide, see the [docs/error-fixes/FIX_ERRORS.md](docs/error-fixes/FIX_ERRORS.md).

### Quick Reference

| Error | Cause | Fix |
|-------|-------|-----|
| `Failed to listen on prefix 'http://localhost:5000/'` | Port 5000 already in use | `Get-Process \| Where-Object {$_.Id -eq <PID>} \| Stop-Process -Force` |
| Student form stuck on `Saving...` | `teacher.id` undefined; `isSubmitting` left `true` | Ensure camelCase JSON in `WebApiConfig.cs`; guard `teacher?.id` in `getHeaders()`; reset `isSubmitting` in `finally` |
| TS2792 — Cannot find module `@angular/router` | `moduleResolution: "node"` incompatible with Angular 18 | Set `"moduleResolution": "bundler"` in `tsconfig.json` |
| CORS error in browser | Backend not running or wrong port | Ensure API running on port 5000; check `WebApiConfig.cs` |
| Login/registration fails | Wrong API base URL | Verify `apiUrl` in service files matches `http://localhost:5000` |

---

## Technology Stack

### Backend
- C# with .NET Framework 4.8
- ASP.NET Web API 5.2
- Entity Framework 6.4 with SQL Server LocalDB
- FluentValidation 11.8
- AutoMapper 10.1
- OWIN/Katana for self-hosting
- ELMAH for error logging
- Swashbuckle 5.6 for Swagger/OpenAPI docs

### Frontend
- Angular 18 (Standalone Components)
- TypeScript 5.2 (`moduleResolution: "bundler"`)
- RxJS 7.8
- Template-driven forms
- Vanilla CSS (responsive, no external UI frameworks)

---

## Future Enhancements

1. **Authentication**: JWT with refresh tokens; bcrypt password hashing
2. **Authorization**: Server-side RBAC enforced on all endpoints
3. **Database**: Migrate to full SQL Server or PostgreSQL
4. **Reporting**: Export to PDF/CSV
5. **Testing**: Unit and integration test suites (backend + frontend)
6. **CI/CD**: GitHub Actions pipeline
7. **Containerization**: Docker + Kubernetes deployment

---

**Last Updated**: May 2026
**Version**: 1.4.0
**Branch**: dev2
**Repository**: https://github.com/AtreusTefo/TrackMyGrade
