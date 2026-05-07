# TrackMyGrade Admin Management - Complete Architecture

## Executive Summary

The TrackMyGrade Admin Management system has been fully implemented with three major features:
- **FEAT-16**: Admin Authentication with JWT
- **FEAT-17**: Teacher and Student Onboarding with referential integrity
- **FEAT-18**: Comprehensive Audit Logging with immutable records

**Key Achievement**: All features implemented with strict data integrity, referential integrity, and data consistency guarantees across both backend (C# .NET Framework 4.8) and frontend (Angular 18).

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        ANGULAR 18 FRONTEND                       │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │  Admin Dashboard │ Teachers │ Students │ Audit Logs       │  │
│  └────────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ AdminAuthService │ AdminApiService │ Route Guards          │  │
│  └────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                         HTTP/JWT Token
┌─────────────────────────────────────────────────────────────────┐
│                    ASP.NET FRAMEWORK 4.8 API                    │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │            AdminController                                 │  │
│  │  /login  /teachers  /students  /audit-logs                │  │
│  └────────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │  AdminService      │  AuditLogService                      │  │
│  │  Business Logic    │  Compliance Logging                   │  │
│  └────────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │            ApplicationDbContext (EF6)                      │  │
│  │  ├─ Teachers        ├─ ClassGroups      ├─ AuditLogs      │  │
│  │  ├─ Students        ├─ Enrollments      └─ Validations    │  │
│  │  └─ Courses         └─ Assignments                         │  │
│  └────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                    SQL Server LocalDB
┌─────────────────────────────────────────────────────────────────┐
│                  DATABASE CONSTRAINTS                            │
│  ├─ Unique Indexes (Email, OMANG, Code)                         │
│  ├─ Foreign Keys (with strategic cascade rules)                  │
│  ├─ Check Constraints (Grade 1-12)                              │
│  └─ Audit Logs Table (write-only, indexed)                      │
└─────────────────────────────────────────────────────────────────┘
```

---

## Layer-by-Layer Implementation

### Layer 1: Presentation (Controllers)

**AdminController** (`TrackMyGradeAPI/Presentation/Controllers/AdminController.cs`)

- `POST /api/admin/login` (No Auth) → Returns JWT
- `GET/POST/DELETE /api/admin/teachers` (Admin Auth)
- `GET/POST/PUT/DELETE /api/admin/students` (Admin Auth)
- `GET/POST /api/admin/courses` (Admin Auth)
- `GET/POST/DELETE /api/admin/class-groups` (Admin Auth)
- `GET /api/admin/audit-logs` (Admin Auth) - Paginated with filters
- `GET /api/admin/audit-logs/entity/{type}/{id}` (Admin Auth)
- `GET /api/admin/audit-logs/user/{email}` (Admin Auth)

**Error Handling**:
- 400 Bad Request - Validation failed, business rule violated
- 401 Unauthorized - JWT invalid or missing
- 404 Not Found - Resource doesn't exist
- 500 Internal Server Error - Unexpected error

### Layer 2: Application Services

**AdminService** (`TrackMyGradeAPI/Application/Services/AdminService.cs`)

**Responsibilities**:
1. Business logic orchestration
2. Input validation delegation (to validators)
3. Referential integrity checks
4. Uniqueness constraint checks
5. Cascade operation coordination
6. Audit logging coordination

**Key Methods**:
- `Login(AdminLoginDto)` - JWT generation
- `CreateTeacher(AdminCreateTeacherDto)` - Validation + DB + Audit
- `DeleteTeacher(int)` - Safety checks + DB + Audit
- `CreateStudent(AdminCreateStudentDto)` - Validation + Referential + DB + Audit
- `UpdateStudent(int, AdminUpdateStudentDto)` - Validation + Audit
- `DeleteStudent(int)` - Cascade + Audit
- `EnrollStudent(int, int)` - Duplicate check + DB + Audit
- `UnenrollStudent(int, int)` - DB + Audit

**AuditLogService** (`TrackMyGradeAPI/Application/Services/AuditLogService.cs`)

**Responsibilities**:
1. Immutable log recording
2. JSON serialization of changes
3. Pagination and filtering
4. Historical queries

**Key Methods**:
- `LogCreate(entityType, entityId, newValues, ...)`
- `LogUpdate(entityType, entityId, oldValues, newValues, ...)`
- `LogDelete(entityType, entityId, oldValues, ...)`
- `GetAuditLogs(AuditLogFilterDto)` - Paginated response
- `GetAuditLogsByEntity(entityType, entityId)`
- `GetAuditLogsByUser(email)`

### Layer 3: Data Access & Models

**ApplicationDbContext** (`TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs`)

```
DbSets:
├─ Teachers (5 fields + 3 activation fields)
├─ Students (8 fields + 3 activation fields)
├─ Courses (3 fields)
├─ ClassGroups (4 FK fields)
├─ StudentEnrollments (2 FK + timestamp)
├─ Assignments (7 fields)
├─ AssignmentSubmissions (6 fields)
└─ AuditLogs (8 fields)

Constraints:
├─ Unique: Email (Teachers, Students)
├─ Unique: OmangOrPassport (Students)
├─ Unique: Code (Courses)
├─ Unique: (StudentId, ClassGroupId) - Enrollments
├─ FK: Student.TeacherId → Teacher.Id (No Cascade)
├─ FK: StudentEnrollment.StudentId → Student.Id (Cascade)
├─ FK: StudentEnrollment.ClassGroupId → ClassGroup.Id (No Cascade)
├─ FK: Assignment.ClassGroupId → ClassGroup.Id (Cascade)
├─ FK: Assignment.CreatedByTeacherId → Teacher.Id (No Cascade)
├─ FK: AssignmentSubmission.StudentId → Student.Id (No Cascade)
└─ FK: AssignmentSubmission.AssignmentId → Assignment.Id (Cascade)

Indexes:
├─ IX_Teacher_Email (Unique)
├─ IX_Student_Email (Unique)
├─ IX_Student_OmangOrPassport (Unique)
├─ IX_Course_Code (Unique)
├─ IX_StudentEnrollment_StudentId_ClassGroupId (Unique)
├─ IX_AuditLog_EntityType_PerformedAt
└─ IX_AuditLog_PerformedBy_PerformedAt
```

### Layer 4: Validation

**AdminValidator** (`TrackMyGradeAPI/Application/Validators/AdminValidator.cs`)

```
Validators:
├─ ValidateCreateTeacher()
│  ├─ First/Last name required, max 100 chars
│  ├─ Email required, valid format, max 255 chars
│  ├─ Phone optional, valid format, max 20 chars
│  └─ Subject optional, max 100 chars
├─ ValidateCreateStudent()
│  ├─ First/Last name required, max 100 chars
│  ├─ Email required, valid format
│  ├─ Phone optional, valid format
│  ├─ OMANG/Passport required, max 9 chars
│  └─ Grade required, 1-12
├─ ValidateUpdateStudent()
│  └─ Same as CreateStudent
├─ ValidateCreateCourse()
│  ├─ Name required, max 200 chars
│  ├─ Code required, max 20 chars (unique)
│  └─ Description optional, max 500 chars
└─ ValidateCreateClassGroup()
   ├─ Name required, max 100 chars
   ├─ GradeLevel required, 1-12
   ├─ CourseId required (must exist)
   └─ TeacherId required (must exist)
```

### Layer 5: Data Transfer Objects

**DTOs** (`TrackMyGradeAPI/Application/DTOs/AdminDto.cs`)

```
Request DTOs:
├─ AdminLoginDto { email, password }
├─ AdminCreateTeacherDto { firstName, lastName, email, phone?, subject? }
├─ AdminCreateStudentDto { firstName, lastName, email, phone?, omangOrPassport, grade, teacherId }
├─ AdminUpdateStudentDto { firstName, lastName, email, phone?, omangOrPassport, grade, teacherId }
├─ CreateCourseDto { name, code, description? }
├─ CreateClassGroupDto { name, gradeLevel, courseId, teacherId }
├─ AuditLogFilterDto { entityType?, action?, performedBy?, startDate?, endDate?, pageNumber, pageSize }
└─ EnrollStudentDto { studentId }

Response DTOs:
├─ AdminResponseDto { email, token }
├─ AdminTeacherDto { id, firstName, lastName, email, phone, subject, isActivated, activationToken?, activatedAt? }
├─ AdminStudentDto { id, studentNumber, firstName, lastName, email, phone, omangOrPassport, grade, teacherId, isActivated, activationToken? }
├─ AuditLogDto { id, entityType, entityId, action, changes, performedBy, performedAt, ipAddress?, userAgent? }
└─ AuditLogPagedResponseDto { totalCount, pageNumber, pageSize, records[] }
```

---

## Data Flow: Create Student Example

```
1. Admin clicks "Create Student" in UI
2. Frontend validates form input locally
   ├─ Email format
   ├─ OMANG length
   └─ Grade range (1-12)

3. POST /api/admin/students (with JWT token)
   Request: {
     firstName: "John",
     lastName: "Doe",
     email: "john@example.com",
     grade: 10,
     teacherId: 5
   }

4. AdminController.CreateStudent()
   ├─ Check JWT token [TokenAuthorize("Admin")]
   ├─ Call AdminService.CreateStudent()

5. AdminService.CreateStudent()
   ├─ AdminValidator.ValidateCreateStudent()
   │  ├─ Email format ✓
   │  ├─ OMANG length ✓
   │  └─ Grade 1-12 ✓
   ├─ Normalize email to lowercase
   ├─ Check email uniqueness in DB
   │  └─ Query: _db.Students.Any(s => s.Email == normalizedEmail)
   ├─ Check OMANG uniqueness in DB
   │  └─ Query: _db.Students.Any(s => s.OmangOrPassport == omang)
   ├─ Verify teacher exists (referential integrity)
   │  └─ Query: _db.Teachers.Any(t => t.Id == request.TeacherId)
   ├─ Create Student entity
   ├─ Add to DbContext
   ├─ SaveChanges() → Single transaction
   ├─ Call _auditLogService.LogCreate()
   │  └─ Create AuditLog record
   │      { action: "Created", entityType: "Student", entityId: 15, 
   │        changes: {firstName, lastName, email, grade, teacherId}, 
   │        performedBy: "admin@trackmygrade.com", performedAt: now }
   └─ Return AdminStudentDto

6. Database Effects
   ├─ Students table: New row inserted
   └─ AuditLogs table: New row inserted

7. Controller returns 201 Created
   Response: {
     id: 15,
     studentNumber: "STU-2026-0015",
     firstName: "John",
     lastName: "Doe",
     email: "john@example.com",
     grade: 10,
     teacherId: 5,
     isActivated: false,
     activationToken: "abc123xyz"
   }

8. Frontend
   ├─ Display success message
   ├─ Refresh student list
   └─ Show activation token to admin
```

---

## Data Flow: Delete Teacher Example

```
1. Admin clicks "Delete" on teacher row
2. Frontend shows confirmation: "Delete teacher 'John Smith'?"
3. On confirmation: DELETE /api/admin/teachers/5

4. AdminController.DeleteTeacher(5)
   ├─ Check JWT token [TokenAuthorize("Admin")]
   ├─ Call AdminService.DeleteTeacher(5)

5. AdminService.DeleteTeacher(5)
   ├─ Find teacher by ID
   │  └─ _db.Teachers.Find(5) → Teacher entity
   ├─ Check for orphaned class groups
   │  ├─ Query: _db.ClassGroups.Count(cg => cg.TeacherId == 5)
   │  └─ If > 0: Throw InvalidOperationException
   │     "Cannot delete teacher: they have 2 class group(s)"
   ├─ Check for assignments
   │  ├─ Query: _db.Assignments.Count(a => a.CreatedByTeacherId == 5)
   │  └─ If > 0: Throw InvalidOperationException
   │     "Cannot delete teacher: they have 3 assignment(s)"
   ├─ Create snapshot for audit
   │  └─ { firstName: "John", lastName: "Smith", email: "john@school.com", subject: "Math" }
   ├─ Remove from DbContext
   ├─ SaveChanges() → Atomic
   ├─ Call _auditLogService.LogDelete()
   │  └─ Create AuditLog record
   │      { action: "Deleted", entityType: "Teacher", entityId: 5,
   │        changes: snapshot, performedBy: "admin@trackmygrade.com", performedAt: now }
   └─ Return

6. Database Effects
   ├─ Teachers table: Row deleted
   └─ AuditLogs table: New row inserted

7. Controller returns 200 OK
   Response: { message: "Teacher deleted." }

8. Frontend
   ├─ Display success message
   └─ Refresh teacher list
```

---

## Data Consistency Guarantees

### 1. Atomicity (ACID)
- All changes committed together via `SaveChanges()`
- If any step fails, entire operation rolled back
- No partial updates

### 2. Consistency
- Unique constraints enforced before INSERT/UPDATE
- Foreign keys validated before INSERT/UPDATE
- Business rules checked before database writes
- Audit logs created only after successful commits

### 3. Isolation
- EF6 DbContext instances are thread-safe
- Each request gets isolated transaction
- No dirty reads or phantom reads

### 4. Durability
- SQL Server persists all commits
- Audit logs permanently recorded
- No data loss on system restart

---

## Error Scenarios & Handling

### Scenario 1: Duplicate Email
```
Input: CreateStudent with email = "admin@trackmygrade.com" (already exists)

Validation Chain:
1. AdminValidator.ValidateCreateStudent() ✓ (format is valid)
2. AdminService checks uniqueness
   └─ _db.Students.Any(s => s.Email == "admin@trackmygrade.com") → true
3. Throws InvalidOperationException
   └─ "A student with this email already exists."

Response:
Status: 400 Bad Request
Body: { message: "A student with this email already exists." }

Audit Impact: No audit log created (error occurred before SaveChanges)
```

### Scenario 2: Invalid Teacher Reference
```
Input: CreateStudent with teacherId = 999 (doesn't exist)

Validation Chain:
1. AdminValidator.ValidateCreateStudent() ✓
2. AdminService checks teacher existence
   └─ _db.Teachers.Any(t => t.Id == 999) → false
3. Throws KeyNotFoundException
   └─ "Teacher with ID 999 not found."

Response:
Status: 400 Bad Request
Body: { message: "Teacher with ID 999 not found." }

Audit Impact: No audit log created (error occurred before SaveChanges)
```

### Scenario 3: Teacher with Active Classes
```
Input: DELETE /api/admin/teachers/5 (has 2 class groups)

Validation Chain:
1. Find teacher ✓
2. Check class groups
   └─ _db.ClassGroups.Count(cg => cg.TeacherId == 5) → 2
3. Throws InvalidOperationException
   └─ "Cannot delete teacher: they have 2 class group(s)."

Response:
Status: 400 Bad Request
Body: { message: "Cannot delete teacher: they have 2 class group(s)." }

Audit Impact: No audit log created (deletion blocked)
```

---

## Frontend Components

### Admin Login Component
- Path: `StudentApp/src/app/components/admin-login/`
- Features:
  - Email and password inputs
  - Client-side validation
  - JWT token storage in localStorage
  - Redirect to dashboard on success
  - Error message display

### Admin Dashboard Component
- Path: `StudentApp/src/app/components/admin-dashboard/`
- Tabs: Teachers | Students | Courses | Classes
- Features:
  - List views with search/filter
  - Create forms with validation
  - Delete with confirmation
  - Edit functionality
  - Error/success notifications

### Audit Logs Component
- Path: `StudentApp/src/app/components/audit-logs/`
- Features:
  - Filter by entity type, action, performer, date range
  - Pagination with page size selection
  - Expandable JSON viewer for changes
  - Color-coded action badges
  - Responsive design

### Services

**AdminAuthService** (`admin-auth.service.ts`)
- Login/logout
- State management (BehaviorSubject)
- LocalStorage persistence
- Observable streams

**AdminApiService** (`admin-api.service.ts`)
- HTTP client wrapper
- JWT token injection in headers
- Teachers CRUD
- Students CRUD
- Courses CRUD
- ClassGroups CRUD
- Audit logs queries

**Route Guards**
- `adminAuthGuard`: Requires JWT token
- `adminGuestGuard`: Prevents logged-in users from login page

---

## JWT Token Structure

```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "admin@trackmygrade.com",
  "role": "Admin",
  "exp": 1715000000,  // 1 hour from issue
  "iat": 1714996400,
  "iss": "TrackMyGrade"
}

Signature: HMAC(header + payload, secret_key)
```

---

## Testing Matrix

| Feature | Unit | Integration | E2E |
|---------|------|-------------|-----|
| Admin Login | ✓ | ✓ | ✓ |
| Teacher Create | ✓ | ✓ | ✓ |
| Teacher Delete (with safety) | ✓ | ✓ | ✓ |
| Student Create | ✓ | ✓ | ✓ |
| Student Update | ✓ | ✓ | ✓ |
| Student Delete (cascade) | ✓ | ✓ | ✓ |
| Enrollment Create | ✓ | ✓ | ✓ |
| Enrollment Duplicate Prevention | ✓ | ✓ | ✓ |
| Audit Log Creation | ✓ | ✓ | ✓ |
| Audit Log Filtering | ✓ | ✓ | ✓ |
| Audit Log Pagination | ✓ | ✓ | ✓ |

---

## Performance Considerations

### Database Queries
- **O(1) lookups**: Using primary keys
- **O(n) scans**: Necessary for uniqueness checks (indexed)
- **Pagination**: Reduces memory usage for large result sets

### Indexes Impact
- Email uniqueness check: O(1) via index
- Audit log queries: O(log n) via composite indexes
- Entity lookups: O(1) via primary key

### Optimization Opportunities (Future)
1. Query caching for read-heavy operations
2. Batch operations for bulk creates
3. Async/await for I/O operations
4. Connection pooling tuning
5. Audit log archival strategy

---

## Security Analysis

### Authentication
- JWT tokens (not session cookies)
- Configurable expiration
- Signed with secret key
- Role-based ("Admin" role)

### Authorization
- `[TokenAuthorize("Admin")]` attribute on all protected endpoints
- Token validation on each request
- No cross-role access

### Input Validation
- Email format validation
- Length constraints
- Character restrictions
- SQL injection prevention (EF6 parameterized queries)

### Data Protection
- Audit logs capture actions (accountability)
- No sensitive data in audit log changes (passwords excluded)
- Timestamps in UTC (no timezone confusion)

### Areas for Enhancement
- 2FA for admin login
- Rate limiting on login endpoint
- Encryption for sensitive audit data
- IP whitelist enforcement
- Admin password rotation policy

---

## Deployment Checklist

### Backend (.NET)
- [ ] Update web.config with admin credentials
- [ ] Set JWT secret in appSettings
- [ ] Configure connection string (LocalDB/SQL Server)
- [ ] Run EF6 migrations or initialize database
- [ ] Verify OWIN middleware setup
- [ ] Test JWT token generation
- [ ] Enable HTTPS in production

### Frontend (Angular)
- [ ] Update API base URL (`http://localhost:5000`)
- [ ] Configure CORS settings (allow StudentApp origin)
- [ ] Test localStorage token persistence
- [ ] Verify route guards work correctly
- [ ] Build production bundle (`ng build --prod`)
- [ ] Deploy to web server

### Database
- [ ] Create AuditLogs table and indexes
- [ ] Verify unique constraints
- [ ] Verify foreign key relationships
- [ ] Check cascade delete triggers
- [ ] Backup database before deployment

### Monitoring
- [ ] Configure ELMAH error logging
- [ ] Monitor admin login attempts
- [ ] Track audit log growth
- [ ] Alert on deletion of sensitive records
- [ ] Regular backup of audit logs

---

## Compliance & Auditing

### Records Kept
- All admin login attempts (successful/failed)
- All Create/Update/Delete operations
- Performer (admin email) and timestamp
- Before/after state for updates
- Snapshots for deletions

### Retention Policy (Recommended)
- Active records: Keep indefinitely
- Audit logs: Keep minimum 7 years
- Failed login attempts: Keep 90 days
- Archive old audit logs to separate table

### Compliance Standards
- GDPR: Right to audit personal data changes
- SOX: Complete audit trail of user actions
- HIPAA: Accountability and non-repudiation
- ISO 27001: Information security controls

---

## Document References

1. **IMPLEMENTATION_ADMIN_FEATURES.md** - Implementation details and code patterns
2. **DATA_INTEGRITY_CONSISTENCY.md** - Architecture for data consistency
3. **CONTEXT_SCOPE_ADMIN_FEATURES.md** - Feature summary and endpoints
4. **AGENTS.md** - Project guidelines and coding standards

---

**Implementation Status**: COMPLETE ✓  
**Version**: 1.0  
**Last Updated**: May 7, 2026  
**Next Review**: Quarterly

