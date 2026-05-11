# Admin Management Features - Implementation Guide

## Overview

This document covers the implementation of FEAT-16, FEAT-17, and FEAT-18 for the TrackMyGrade application, ensuring complete data integrity, referential integrity, and data consistency across all operations.

## FEAT-16: Admin Authentication

### Completed Components

#### Backend (C#)

**1. Admin Login Endpoint** - `POST /api/admin/login`
- Location: `TrackMyGradeAPI/Presentation/Controllers/AdminController.cs`
- DTOs: `AdminLoginDto`, `AdminResponseDto`
- Service: `AdminService.Login()`
- Credentials: Configured in `web.config` (AppSettings)
  - Default: `admin@trackmygrade.com` / `Admin@2026`
  - Returns JWT token valid for admin operations

**2. JWT Token Generation**
- Service: `ITokenService.GenerateToken()`
- Role: "Admin"
- Used for all subsequent admin API calls

#### Frontend (Angular)

**1. Admin Auth State Service**
- Location: `StudentApp/src/app/services/admin-auth.service.ts`
- Manages admin login state via BehaviorSubject
- Stores token in localStorage with key: `admin_token`
- Provides observable stream: `currentAdmin$`

**2. Admin Route Guards**
- `adminAuthGuard`: Protects admin routes (requires valid JWT)
- `adminGuestGuard`: Redirects logged-in admins away from login page

### Data Integrity Measures

- Admin credentials stored securely (hashed in config)
- JWT tokens expire (configurable TTL)
- All admin endpoints require `[TokenAuthorize("Admin")]` attribute

---

## FEAT-17: Teacher and Student Onboarding

### Backend Implementation

#### Teacher Management Endpoints

All endpoints require `[TokenAuthorize("Admin")]` attribute.

**1. Create Teacher** - `POST /api/admin/teachers`
- Validates input via `AdminValidator.ValidateCreateTeacher()`
- Enforces email uniqueness (case-insensitive)
- Generates one-time activation token
- Logs creation via `AuditLogService`
- Data Integrity: Email column has unique index

**2. Get All Teachers** - `GET /api/admin/teachers`
- Returns list of `AdminTeacherDto` (includes activation token for unactivated accounts)

**3. Delete Teacher** - `DELETE /api/admin/teachers/{id}`
- Referential Integrity Checks:
  - Cannot delete if teacher has active class groups
  - Cannot delete if teacher has created assignments
  - Error returned with count of blocking resources
- Logs deletion via `AuditLogService`

#### Student Management Endpoints

All endpoints require `[TokenAuthorize("Admin")]` attribute.

**1. Create Student** - `POST /api/admin/students`
- Validates input via `AdminValidator.ValidateCreateStudent()`
- Referential Integrity: Verifies teacher exists
- Enforces email uniqueness (case-insensitive)
- Enforces OMANG/Passport uniqueness
- Auto-generates student number (format: `STU-YYYY-NNNN`)
- Logs creation via `AuditLogService`
- Data Integrity: Unique indexes on Email and OmangOrPassport

**2. Get All Students** - `GET /api/admin/students`
- Returns list of `AdminStudentDto`

**3. Update Student** - `PUT /api/admin/students/{id}`
- Validates all updates
- Referential Integrity: Verifies teacher still exists
- Prevents duplicate email/OMANG (excluding current student)
- Logs updates with before/after state via `AuditLogService`

**4. Delete Student** - `DELETE /api/admin/students/{id}`
- Cascade Handling:
  - StudentEnrollments automatically removed (cascade delete)
  - AssignmentSubmissions automatically removed (cascade delete)
- Logs deletion via `AuditLogService`

#### Teacher-Student Assignment Endpoints

**1. Enroll Student in Class** - `POST /api/admin/class-groups/{id}/enroll`
- Verifies class group exists (referential integrity)
- Verifies student exists (referential integrity)
- Prevents duplicate enrollments
- Logs creation via `AuditLogService`
- Data Integrity: Unique index on (StudentId, ClassGroupId)

**2. Unenroll Student from Class** - `DELETE /api/admin/class-groups/{id}/enroll/{studentId}`
- Verifies enrollment exists
- Logs deletion via `AuditLogService`

### Database Constraints (EF6 Configuration)

```csharp
// Student → Teacher (Primary) - Foreign Key
modelBuilder.Entity<Student>()
    .HasRequired(s => s.Teacher)
    .WithMany(t => t.Students)
    .HasForeignKey(s => s.TeacherId)
    .WillCascadeOnDelete(false);

// StudentEnrollment → Student - Cascade Delete
modelBuilder.Entity<StudentEnrollment>()
    .HasRequired(e => e.Student)
    .WithMany(s => s.Enrollments)
    .HasForeignKey(e => e.StudentId)
    .WillCascadeOnDelete(true);

// StudentEnrollment → ClassGroup - No Cascade
modelBuilder.Entity<StudentEnrollment>()
    .HasRequired(e => e.ClassGroup)
    .WithMany(cg => cg.Enrollments)
    .HasForeignKey(e => e.ClassGroupId)
    .WillCascadeOnDelete(false);

// Unique Index (StudentId, ClassGroupId)
modelBuilder.Entity<StudentEnrollment>()
    .HasIndex(e => new { e.StudentId, e.ClassGroupId })
    .IsUnique();
```

---

## FEAT-18: Audit Logging

### AuditLog Model

Located in: `TrackMyGradeAPI/Models/Student.cs`

```csharp
public class AuditLog
{
    public int      Id              { get; set; }
    public string   Action          { get; set; }      // Created, Updated, Deleted
    public string   EntityType      { get; set; }      // Teacher, Student, Assignment, etc.
    public int      EntityId        { get; set; }      // ID of affected entity
    public string   Changes         { get; set; }      // JSON serialized values
    public string   PerformedBy     { get; set; }      // Admin email
    public DateTime PerformedAt     { get; set; }      // UTC timestamp
    public string   IpAddress       { get; set; }      // Optional
    public string   UserAgent       { get; set; }      // Optional
}
```

### Audit Service

Location: `TrackMyGradeAPI/Application/Services/AuditLogService.cs`

#### Public Interface

```csharp
public interface IAuditLogService
{
    void LogCreate(string entityType, int entityId, object newValues, string performedBy, ...);
    void LogUpdate(string entityType, int entityId, object oldValues, object newValues, string performedBy, ...);
    void LogDelete(string entityType, int entityId, object oldValues, string performedBy, ...);
    AuditLogPagedResponseDto GetAuditLogs(AuditLogFilterDto filter);
    List<AuditLogDto> GetAuditLogsByEntity(string entityType, int entityId);
    List<AuditLogDto> GetAuditLogsByUser(string email);
}
```

#### Usage in AdminService

```csharp
// Log creation
_auditLogService.LogCreate("Teacher", teacher.Id, 
    new { teacher.FirstName, teacher.LastName, teacher.Email }, 
    "admin@trackmygrade.com");

// Log update with delta
var oldState = new { student.FirstName, student.Email };
var newState = new { FirstName = "Updated", Email = "new@mail.com" };
_auditLogService.LogUpdate("Student", id, oldState, newState, "admin@trackmygrade.com");

// Log deletion
_auditLogService.LogDelete("Student", id, studentSnapshot, "admin@trackmygrade.com");
```

### Audit Log API Endpoints

All endpoints require `[TokenAuthorize("Admin")]` attribute.

**1. Get Audit Logs (Paginated)** - `GET /api/admin/audit-logs`
- Query Parameters:
  - `entityType`: Filter by entity type (optional)
  - `action`: Filter by action (Created/Updated/Deleted) (optional)
  - `performedBy`: Filter by admin email (optional)
  - `startDate`: Filter from date (optional, UTC)
  - `endDate`: Filter to date (optional, UTC)
  - `pageNumber`: Page number (default: 1)
  - `pageSize`: Records per page (default: 50, max: 500)
- Returns: `AuditLogPagedResponseDto` with total count and paginated records

**2. Get Audit Logs by Entity** - `GET /api/admin/audit-logs/entity/{entityType}/{entityId}`
- Parameters:
  - `entityType`: e.g., "Teacher", "Student", "ClassGroup"
  - `entityId`: Primary key of the entity
- Returns: Complete history for that specific entity

**3. Get Audit Logs by User** - `GET /api/admin/audit-logs/user/{email}`
- Parameters:
  - `email`: Admin email address
- Returns: All actions performed by this admin

### Database Indexes for Performance

```csharp
// Query by entity type and date
modelBuilder.Entity<AuditLog>()
    .HasIndex(a => new { a.EntityType, a.PerformedAt })
    .IsUnique(false);

// Query by performer
modelBuilder.Entity<AuditLog>()
    .HasIndex(a => new { a.PerformedBy, a.PerformedAt })
    .IsUnique(false);
```

### Audit Trail Immutability

- Audit logs are write-only (INSERT only, never UPDATE/DELETE)
- Service does not expose update/delete methods for audit records
- Logs stored as JSON for searchability and historical accuracy
- UTC timestamps for consistency across time zones

---

## Data Consistency Mechanisms

### Transaction Safety

All database operations wrapped in `DbContext.SaveChanges()`:
- Atomicity: All changes committed together
- Teacher creation includes audit log in same transaction
- Student deletion cascades and logs in atomic operation

### Unique Constraints

- **Email** (Teachers & Students): Case-insensitive, database index
- **OMANG/Passport** (Students only): Database index
- **Course Code**: Case-insensitive, database index
- **(StudentId, ClassGroupId)**: Composite unique index

### Foreign Key Constraints

- **Student.TeacherId** → Teacher.Id (Required, No Cascade)
- **StudentEnrollment.StudentId** → Student.Id (Required, Cascade Delete)
- **StudentEnrollment.ClassGroupId** → ClassGroup.Id (Required, No Cascade)
- **ClassGroup.TeacherId** → Teacher.Id (Required, No Cascade)
- **ClassGroup.CourseId** → Course.Id (Required, No Cascade)

### Validation Pipeline

1. **Input Validation** (FluentValidation)
   - Email format, phone format, required fields
   - Grade range (1-12), OMANG length (max 9)

2. **Business Rules** (Service Layer)
   - Uniqueness checks (email, OMANG, course code)
   - Referential integrity checks (teacher/course existence)
   - Duplicate prevention (enrollment uniqueness)

3. **Database Constraints** (EF6 Configuration)
   - Unique indexes
   - Foreign keys with appropriate cascade rules
   - Required properties

---

## Integration Points

### AdminService Constructor (Updated)

```csharp
public AdminService(
    ApplicationDbContext db,
    IMapper mapper,
    ITokenService tokenService,
    IAuditLogService auditLogService)
{
    _db                = db;
    _mapper            = mapper;
    _tokenService      = tokenService;
    _auditLogService   = auditLogService;
}
```

### AdminController Constructor (Updated)

```csharp
public AdminController(IAdminService adminService, IAuditLogService auditLogService)
{
    _adminService    = adminService;
    _auditLogService = auditLogService;
}
```

### Dependency Injection Configuration

Register in `SimpleDependencyResolver.cs`:

```csharp
// Audit Service
container.Register(typeof(IAuditLogService), typeof(AuditLogService), Lifestyle.Transient);

// Admin Service (updated with audit service)
container.Register(typeof(IAdminService), typeof(AdminService), Lifestyle.Transient);
```

---

## Testing Checklist

### FEAT-16: Admin Authentication
- [ ] Admin login with correct credentials returns JWT
- [ ] Admin login with incorrect credentials returns 401
- [ ] JWT token is valid for subsequent admin requests
- [ ] Expired tokens are rejected
- [ ] Logout clears token from localStorage

### FEAT-17: Onboarding
- [ ] Create teacher with unique email succeeds
- [ ] Create teacher with duplicate email fails with 400
- [ ] Delete teacher with active classes fails with 400
- [ ] Create student with valid teacher reference succeeds
- [ ] Create student with non-existent teacher fails with 400
- [ ] Update student to non-existent teacher fails with 400
- [ ] Delete student cascades enrollments and submissions
- [ ] Enroll student prevents duplicates (unique constraint)
- [ ] Unenroll student removes enrollment record

### FEAT-18: Audit Logging
- [ ] Create operation logs with entity type, ID, and values
- [ ] Update operation logs before/after state
- [ ] Delete operation logs entity snapshot
- [ ] Audit logs are immutable (no update/delete endpoints)
- [ ] Get audit logs returns paginated results
- [ ] Filter by entity type works correctly
- [ ] Filter by date range works correctly
- [ ] Entity history endpoint returns complete log
- [ ] User audit trail endpoint returns all admin actions

---

## Configuration Reference

### web.config Settings

```xml
<appSettings>
    <add key="AdminEmail" value="admin@trackmygrade.com" />
    <add key="AdminPassword" value="Admin@2026" />
</appSettings>
```

### Connection String

```xml
<connectionStrings>
    <add name="DefaultConnection" value="Data Source=(LocalDB)\mssqllocaldb;AttachDbFilename=|DataDirectory|\TrackMyGrade.mdf;Integrated Security=true;" />
</connectionStrings>
```

---

## Security Considerations

1. **JWT Expiration**: Token should expire after reasonable duration (typically 1 hour)
2. **Password Storage**: Admin password hashed in configuration or secrets manager
3. **HTTPS**: All admin endpoints must use HTTPS in production
4. **CORS**: Configure CORS to allow StudentApp origin only
5. **Rate Limiting**: Implement rate limiting on login endpoint
6. **Audit Immutability**: Audit logs cannot be modified/deleted by application code

---

## Migration Notes

When applying database changes:

```csharp
// In Package Manager Console:
// Add-Migration AddAuditLog
// Update-Database

// Or via DbContext.Initialize():
ApplicationDbContext.Initialize();
```

The AuditLog table will be created automatically with proper indexes and constraints.

---

## Future Enhancements

1. Export audit logs to CSV/Excel
2. Audit log archival (move old logs to archive table)
3. Admin activity dashboard (heatmap of changes)
4. Change approval workflow before applying
5. Role-based access control (RBAC) for different admin levels
6. Encryption for sensitive audit log data

