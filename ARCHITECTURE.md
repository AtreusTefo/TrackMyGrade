# TrackMyGrade - System Architecture

## High-Level Architecture Overview

TrackMyGrade follows a **Clean Architecture with Separation of Concerns (SoC)** pattern, ensuring the backend API and frontend are completely decoupled and independently deployable.

```
┌─────────────────────────────────────────────────────────────┐
│                     Angular 18 SPA                          │
│        (Standalone Components - No Module Required)         │
├─────────────────────────────────────────────────────────────┤
│  Login    Register    StudentList    StudentForm    Detail  │
│  Components - Services - Models - Routing - Styles         │
├─────────────────────────────────────────────────────────────┤
│              HttpClient with CORS Support                   │
├─────────────────────────────────────────────────────────────┤
│  HTTP Calls (JSON) to REST API on port 5000               │
└─────────────────────────────────────────────────────────────┘
                        ⬇️ HTTP/JSON
┌─────────────────────────────────────────────────────────────┐
│             ASP.NET Framework Web API                       │
│      (Self-hosted via OWIN/Katana on port 5000)           │
├─────────────────────────────────────────────────────────────┤
│  Presentation Layer                                         │
│  • TeachersController (Register, Login, Profile)           │
│  • StudentsController (CRUD operations)                    │
│  • Input validation via FluentValidation                   │
├─────────────────────────────────────────────────────────────┤
│  Business Logic Layer                                       │
│  • TeacherService (Authentication logic)                   │
│  • StudentService (CRUD + Calculations)                    │
├─────────────────────────────────────────────────────────────┤
│  Data Access Layer                                          │
│  • ApplicationDbContext (EF Core)                          │
│  • Database: SQLite In-Memory                              │
├─────────────────────────────────────────────────────────────┤
│  Cross-cutting Concerns                                     │
│  • AutoMapper (DTO ↔ Entity mapping)                       │
│  • CORS (Enable Angular to call API)                       │
└─────────────────────────────────────────────────────────────┘
```

## Detailed Architecture Layers

### 1. Frontend Layer (Angular 18)

#### Components (Standalone)
```
LoginComponent
├─ Handles teacher authentication
├─ Validates email & password
└─ Stores token in localStorage

RegisterComponent
├─ Teacher registration form
├─ Validates all fields (FluentValidation rules mirrored)
└─ Redirects to login on success

StudentListComponent
├─ Displays all students in table
├─ Shows calculated metrics
├─ Provides action buttons (View, Edit, Delete)
└─ Handles deletion with confirmation

StudentFormComponent
├─ Shared component for Create & Edit
├─ Real-time calculation display
├─ Validates input before submission
└─ Updates URL on save

StudentDetailComponent
├─ Full student information view
├─ Performance analysis
├─ Edit and Delete shortcuts
```

#### Services
```
AuthService
├─ register(data) → Observable<Teacher>
├─ login(data) → Observable<Teacher>
├─ logout() → void
├─ getCurrentTeacher() → Teacher | null
├─ getToken() → string | null
├─ isAuthenticated() → boolean
└─ Manages currentTeacher$ BehaviorSubject

StudentService
├─ getAllStudents() → Observable<Student[]>
├─ getStudentById(id) → Observable<Student>
├─ createStudent(data) → Observable<Student>
├─ updateStudent(id, data) → Observable<Student>
├─ deleteStudent(id) → Observable<any>
└─ Passes X-TeacherId header on all requests
```

#### Routing
```
/login                        → LoginComponent (public)
/register                     → RegisterComponent (public)
/                            → StudentListComponent (guarded)
/create                      → StudentFormComponent (guarded)
/edit/:id                    → StudentFormComponent (guarded)
/detail/:id                  → StudentDetailComponent (guarded)
```

#### Authentication Guard
- Checks if user has valid token
- Redirects unauthenticated users to /login
- Tokens stored in localStorage

#### Models (TypeScript Interfaces)
```typescript
Teacher {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  subject: string;
  token: string;
}

Student {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  grade: number;
  assessment1: number;
  assessment2: number;
  assessment3: number;
  total: number;
  average: number;
  percentage: number;
  performanceLevel: string;
}
```

### 2. Presentation Layer (ASP.NET Web API Controllers)

#### TeachersController
```csharp
[POST] /api/teachers/register
├─ Input: TeacherRegisterDto
├─ Validation: FluentValidation
├─ Output: TeacherResponseDto (with token)
└─ Stores in SQLite in-memory DB

[POST] /api/teachers/login
├─ Input: TeacherLoginDto
├─ Validation: Credentials check
├─ Output: TeacherResponseDto (with new token)
└─ Regenerates token on each login

[GET] /api/teachers/{id}
├─ Input: Teacher ID
├─ Output: TeacherResponseDto
└─ Returns current teacher profile
```

#### StudentsController
```csharp
[GET] /api/students
├─ Query: X-TeacherId header
├─ Output: List<StudentResponseDto>
└─ Filters by teacher ID for isolation

[GET] /api/students/{id}
├─ Input: Student ID
├─ Validation: Ownership check (teacher must own)
└─ Output: StudentResponseDto with calculated fields

[POST] /api/students
├─ Input: StudentCreateDto
├─ Validation: FluentValidation rules
├─ Process: Creates entity, calculates totals
└─ Output: StudentResponseDto

[PUT] /api/students/{id}
├─ Input: StudentUpdateDto
├─ Validation: FluentValidation + ownership
├─ Process: Updates all fields + recalculates
└─ Output: StudentResponseDto

[DELETE] /api/students/{id}
├─ Validation: Ownership check
└─ Process: Removes student + cascades (if any FK exists)
```

#### DTOs (Data Transfer Objects)
```
Request DTOs (Input)
├─ TeacherRegisterDto (firstName, lastName, email, phone, subject, password)
├─ TeacherLoginDto (email, password)
├─ StudentCreateDto (firstName, lastName, email, phone, grade, assessment1-3)
└─ StudentUpdateDto (same as create)

Response DTOs (Output)
├─ TeacherResponseDto (includes token)
└─ StudentResponseDto (includes calculated fields)

Purpose: Decouple API contract from DB entities
```

### 3. Business Logic Layer (Services)

#### TeacherService
```csharp
Register(TeacherRegisterDto)
├─ Validates no duplicate email
├─ Creates new Teacher entity
├─ Generates unique token (Guid)
├─ Hashes password (note: currently plain text)
├─ Persists to DB
└─ Returns mapped response

Login(TeacherLoginDto)
├─ Finds teacher by email/password match
├─ Regenerates token on each login
├─ Throws UnauthorizedAccessException on failure
└─ Returns updated teacher response

GetById(int id)
├─ Retrieves teacher profile
└─ Used for profile endpoints
```

#### StudentService
```csharp
GetAllByTeacher(int teacherId)
├─ Filters students by teacher
└─ Maps to response DTOs

GetById(int id, int teacherId)
├─ Validates ownership
└─ Returns detailed student with calculations

Create(StudentCreateDto, int teacherId)
├─ Sets teacherId from context
├─ Saves to DB
├─ Calculations happen in entity
└─ Returns mapped response

Update(int id, StudentUpdateDto, int teacherId)
├─ Validates ownership before update
├─ Merges updated fields
├─ DB auto-recalculates values
└─ Returns updated response

Delete(int id, int teacherId)
├─ Validates ownership
└─ Removes from DB
```

### 4. Data Access Layer (EF Core)

#### ApplicationDbContext
```csharp
DbSet<Teacher> Teachers
DbSet<Student> Students

OnModelCreating()
├─ Configures relationships
├─ Sets max lengths
├─ Cascading delete rules
└─ Seeds data if needed

Initialize()
├─ Called in Global.asax Application_Start
├─ Creates database if not exists
└─ Initializes schema
```

#### Database Schema
```sql
Teachers
├─ Id (PK)
├─ FirstName (string, required)
├─ LastName (string, required)
├─ Email (string, unique, required)
├─ Phone (string, 8 digits)
├─ Subject (string, max 100)
├─ Password (string, plain text)
└─ Token (string)

Students
├─ Id (PK)
├─ TeacherId (FK → Teachers.Id)
├─ FirstName (string, required)
├─ LastName (string, required)
├─ Email (string, required)
├─ Phone (string, 8 digits)
├─ Grade (int, 1-12)
├─ Assessment1 (int, 0-20)
├─ Assessment2 (int, 0-20)
└─ Assessment3 (int, 0-20)

Relationships
└─ One-to-Many: One Teacher has many Students
   ├─ Cascade delete enabled
   └─ Teacher entity not directly exposed in responses
```

### 5. Validation Layer (FluentValidation)

#### Validators
```csharp
TeacherRegisterValidator
├─ FirstName: Required, 2-50 chars
├─ LastName: Required, 2-50 chars
├─ Email: Required, valid format
├─ Phone: Required, 8 digits only
├─ Subject: Required, max 100 chars
└─ Password: Required, 6-20 chars

StudentCreateValidator / StudentUpdateValidator
├─ FirstName: Required, 2-50 chars
├─ LastName: Required, 2-50 chars
├─ Email: Required, valid format
├─ Phone: Required, 8 digits only
├─ Grade: Required, 1-12
├─ Assessments (1-3): Required, 0-20 range
```

**Validation Flow**
```
Frontend (Client-side) → Backend Validation
├─ Client validates before sending request
├─ Backend validates again (never trust client)
└─ Returns 400 BadRequest with error details if invalid
```

### 6. Mapping Layer (AutoMapper)

#### MappingProfile
```csharp
TeacherRegisterDto → Teacher (registration)
Teacher → TeacherResponseDto (with token)

StudentCreateDto → Student (creation)
StudentUpdateDto → Student (update)
Student → StudentResponseDto (response)
├─ Maps calculated properties
├─ Total: Assessment1 + Assessment2 + Assessment3
├─ Average: Total / 3
├─ Percentage: (Total / 60) * 100
└─ PerformanceLevel: Computed based on percentage
```

**Benefits**
- DTOs don't expose internal fields
- Calculations happen automatically in mapping
- Clean separation between API contract and DB schema

### 7. Cross-Cutting Concerns

#### CORS (Cross-Origin Resource Sharing)
```csharp
EnableCorsAttribute("http://localhost:4200", "*", "*")
├─ Allows Angular app to make requests
├─ Permits all HTTP methods
├─ Allows all headers
└─ For production: specify exact origins & methods
```

#### Error Handling
```
Frontend
├─ Catches HttpClientError
├─ Displays global error banner
└─ Logs to console

Backend
├─ Try-catch in controllers
├─ Returns BadRequest for validation errors
├─ Returns 500 for unexpected errors
└─ All errors return JSON
```

#### Calculations Engine
```csharp
Student Model Computed Properties
├─ Total = Assessment1 + Assessment2 + Assessment3
├─ Average = Total / 3.0 (decimal for precision)
├─ Percentage = (Total / 60.0) * 100
└─ PerformanceLevel
   ├─ < 50%: "Needs Support"
   ├─ 50-55%: "Satisfactory"
   ├─ 56-75%: "Good"
   └─ > 75%: "Excellent"

Calculation Flow
├─ Client: Real-time on form (for preview)
├─ Backend: When student entity saves
└─ Response: Included in StudentResponseDto
```

## Data Flow Examples

### Example 1: User Registration & Login

```
User clicks Register
  ↓
[Angular] RegisterComponent validates input
  ↓
[Angular] AuthService.register() sends POST /api/teachers/register
  ↓
[Backend] TeachersController receives request
  ↓
[Backend] Validates TeacherRegisterDto with TeacherRegisterValidator
  ↓
[Backend] TeacherService.Register() creates Teacher entity
  ↓
[Backend] Generates token, saves to SQLite in-memory DB
  ↓
[Backend] Maps Teacher → TeacherResponseDto
  ↓
[Backend] Returns 200 OK with TeacherResponseDto
  ↓
[Angular] Stores teacher info and token in localStorage
  ↓
[Angular] Router redirects to /login
  ↓
User logs in
  ↓
[Angular] LoginComponent calls AuthService.login()
  ↓
[Backend] Similar flow, validates credentials
  ↓
[Backend] Regenerates token on successful login
  ↓
[Angular] Sets currentTeacher$ BehaviorSubject
  ↓
Navbar shows teacher name, routes now guarded
```

### Example 2: Create & View Student

```
User clicks "Add Student"
  ↓
[Angular] Router navigates to /create
  ↓
[Angular] StudentFormComponent loads (isEditMode = false)
  ↓
User fills form → On assessment change: calculateValues()
  ↓
Real-time display shows Total, Average, Percentage, Performance Level
  ↓
User clicks Submit
  ↓
[Angular] Validates all fields locally
  ↓
[Angular] StudentService.createStudent() sends POST /api/students
  ↓
  ⚠️ [Angular] Includes header: X-TeacherId: currentTeacher.id
  ↓
[Backend] StudentsController.Create() receives request
  ↓
[Backend] Validates StudentCreateDto with StudentCreateValidator
  ↓
[Backend] StudentService.Create() creates Student entity
  ↓
  ⚠️ [Backend] Sets TeacherId from header
  ↓
[Backend] Saves to DB, Entity calculates properties
  ↓
[Backend] Maps Student → StudentResponseDto (includes calculations)
  ↓
[Backend] Returns 201 Created with StudentResponseDto
  ↓
[Angular] Redirects to / (StudentListComponent)
  ↓
[Angular] StudentListComponent.ngOnInit() loads students
  ↓
[Angular] StudentService.getAllStudents() sends GET /api/students
  ↓
  ⚠️ [Angular] Includes header: X-TeacherId: currentTeacher.id
  ↓
[Backend] Returns filtered students for that teacher
  ↓
Table rendered with all students and their calculations
```

### Example 3: Edit Student

```
User clicks Edit button on student row
  ↓
[Angular] Router navigates to /edit/{id}
  ↓
[Angular] StudentFormComponent loads (isEditMode = true)
  ↓
[Angular] StudentService.getStudentById() fetches current data
  ↓
Form populates with existing values
  ↓
User modifies assessments
  ↓
Real-time calculation updates
  ↓
User clicks Update
  ↓
[Angular] Validates and sends PUT /api/students/{id}
  ↓
[Backend] Validates StudentUpdateDto
  ↓
[Backend] StudentService.Update() merges changes
  ↓
[Backend] DB auto-recalculates properties
  ↓
Returns updated StudentResponseDto
  ↓
[Angular] Redirects to /detail/{id}
  ↓
StudentDetailComponent displays updated values
```

## Security Considerations (Current & Production)

### Current Implementation (Development)
```
✅ Basic auth (email/password)
✅ Simple tokens (GUID)
✅ Input validation
✅ CORS configured
❌ No HTTPS
❌ Plain text passwords
❌ No token expiration
❌ No authorization enforcement
❌ No rate limiting
```

### Production Recommendations
```
Should implement:
✅ HTTPS/TLS encryption
✅ Password hashing (bcrypt, Argon2)
✅ JWT with expiration & refresh tokens
✅ Role-based access control (RBAC)
✅ Request signing & verification
✅ Rate limiting & DDoS protection
✅ API key management
✅ Audit logging
✅ SQL injection prevention (using parameterized queries)
✅ CORS whitelist (specific origins)
```

## Performance Considerations

### Database
- In-memory SQLite: Very fast, no network latency
- No query optimization needed for small datasets
- All data fits in memory

### Caching Strategy
```
Frontend
├─ Cache student list in component
└─ Invalidate on create/update/delete

Backend
├─ Context-level EF Core tracking optimization
└─ No additional caching layer needed
```

### Response Time Target
- API responses: < 500ms (development)
- Client rendering: < 100ms
- Total page load: < 2 seconds

## Deployment Architecture

### Development
```
Backend: Visual Studio IIS Express (port 5000)
Frontend: Angular dev server (port 4200)
Database: SQLite in-memory
```

### Production (Example)
```
Backend
├─ Host on IIS with Application Pool
├─ SQL Server database
├─ HTTPS enabled
└─ Load balanced if needed

Frontend
├─ Serve from CDN or static host
├─ Minified & bundled
└─ Cache busting enabled

Communication
└─ Both use HTTPS with valid certificates
```

## Testing Strategy

### Backend Unit Tests (Future)
```csharp
TeacherServiceTests
├─ RegisterTeacher_WithValidData_CreatesTeacher
├─ RegisterTeacher_WithDuplicateEmail_ThrowsException
├─ LoginTeacher_WithValidCredentials_ReturnsToken
└─ LoginTeacher_WithInvalidCredentials_ThrowsException

StudentServiceTests
├─ CreateStudent_WithValidData_ReturnsStudent
├─ GetStudentById_WithOwnedStudent_ReturnsStudent
├─ GetStudentById_WithUnownedStudent_ThrowsException
└─ DeleteStudent_WithValidId_RemovesStudent

ValidatorTests
├─ TeacherRegisterValidator_WithInvalidEmail_Fails
├─ StudentCreateValidator_WithAssessmentOutOfRange_Fails
```

### Frontend Component Tests (Future)
```typescript
LoginComponentTests
├─ Should display login form
├─ Should validate email before submit
├─ Should call authService.login() on submit
└─ Should redirect to list on success

StudentListComponentTests
├─ Should load and display students
├─ Should show performance badges
├─ Should call delete API on confirmation
└─ Should reload list on delete
```

---

## Summary

The TrackMyGrade architecture ensures:
- **Separation of Concerns**: Clean layers with well-defined responsibilities
- **Loose Coupling**: Frontend and backend are independent
- **Reusability**: Services and components can be reused
- **Testability**: Each layer can be unit tested in isolation
- **Maintainability**: Clear structure makes future enhancements easy
- **Scalability**: Foundation for adding more features
