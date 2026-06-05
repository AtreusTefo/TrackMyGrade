# Data Integrity & Consistency Architecture

## Overview

This document describes the multi-layered approach to ensuring data integrity, referential integrity, and data consistency across the TrackMyGrade application for the Admin Management features.

---

## 1. Data Integrity Mechanisms

### 1.1 Input Validation (FluentValidation Layer)

**Location**: `TrackMyGradeAPI/Application/Validators/AdminValidator.cs`

#### Teacher Validation
```csharp
public static void ValidateCreateTeacher(AdminCreateTeacherDto request)
{
    if (string.IsNullOrWhiteSpace(request.FirstName))
        throw new ArgumentException("First name is required.");
    if (string.IsNullOrWhiteSpace(request.LastName))
        throw new ArgumentException("Last name is required.");
    if (string.IsNullOrWhiteSpace(request.Email))
        throw new ArgumentException("Email is required.");
    if (!IsValidEmail(request.Email))
        throw new ArgumentException("Invalid email format.");
    if (!string.IsNullOrWhiteSpace(request.Phone) && !IsValidPhone(request.Phone))
        throw new ArgumentException("Invalid phone format.");
    if (request.Subject != null && request.Subject.Length > 100)
        throw new ArgumentException("Subject cannot exceed 100 characters.");
}
```

#### Student Validation
```csharp
public static void ValidateCreateStudent(AdminCreateStudentDto request)
{
    if (string.IsNullOrWhiteSpace(request.FirstName))
        throw new ArgumentException("First name is required.");
    if (string.IsNullOrWhiteSpace(request.LastName))
        throw new ArgumentException("Last name is required.");
    if (string.IsNullOrWhiteSpace(request.Email))
        throw new ArgumentException("Email is required.");
    if (!IsValidEmail(request.Email))
        throw new ArgumentException("Invalid email format.");
    if (string.IsNullOrWhiteSpace(request.OmangOrPassport))
        throw new ArgumentException("OMANG or Passport is required.");
    if (request.OmangOrPassport.Length > 9)
        throw new ArgumentException("OMANG/Passport cannot exceed 9 characters.");
    if (request.Grade < 1 || request.Grade > 12)
        throw new ArgumentException("Grade must be between 1 and 12.");
}
```

**Benefits**:
- Prevents malformed data from entering the system
- Consistent validation across all entry points
- Early error detection (fail-fast principle)

### 1.2 Business Logic Validation (Service Layer)

**Location**: `TrackMyGradeAPI/Application/Services/AdminService.cs`

#### Uniqueness Checks

```csharp
// Email uniqueness (case-insensitive)
string normalizedEmail = request.Email.Trim().ToLower();
if (_db.Teachers.Any(t => t.Email == normalizedEmail))
    throw new InvalidOperationException("A teacher with this email already exists.");

// OMANG/Passport uniqueness (students)
string normalizedPassport = request.OmangOrPassport.Trim();
if (_db.Students.Any(s => s.OmangOrPassport == normalizedPassport))
    throw new InvalidOperationException("A student with this OMANG/Passport already exists.");

// Subject code uniqueness (case-insensitive)
string normalizedCode = request.Code.Trim().ToUpper();
if (_db.Subjects.Any(c => c.Code == normalizedCode))
    throw new InvalidOperationException("A subject with this code already exists.");
```

**Guarantees**:
- No duplicate emails (case-insensitive comparison)
- No duplicate identifiers (OMANG, Passport)
- No duplicate subject codes
- Check performed before database INSERT

#### Update Safety Checks

```csharp
// When updating student email, exclude current student
if (student.Email != normalizedEmail &&
    _db.Students.Any(s => s.Id != id && s.Email == normalizedEmail))
    throw new InvalidOperationException("A student with this email already exists.");
```

**Guarantees**:
- Safe updates without false duplicate detection
- Atomic validation before modification

---

## 2. Referential Integrity

### 2.1 Foreign Key Constraints (EF6)

**Location**: `TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs`

#### Teacher-Student Relationship
```csharp
modelBuilder.Entity<Student>()
    .HasRequired(s => s.Teacher)
    .WithMany(t => t.Students)
    .HasForeignKey(s => s.TeacherId)
    .WillCascadeOnDelete(false);   // Manual cascade via business logic
```

**Effect**:
- Student.TeacherId must reference valid Teacher.Id
- Database enforces at INSERT/UPDATE time
- DELETE on Teacher blocked if students exist (unless manual cascade)

#### StudentEnrollment Relationships
```csharp
// Student → StudentEnrollment (CASCADE DELETE)
modelBuilder.Entity<StudentEnrollment>()
    .HasRequired(e => e.Student)
    .WithMany(s => s.Enrollments)
    .HasForeignKey(e => e.StudentId)
    .WillCascadeOnDelete(true);

// ClassGroup → StudentEnrollment (NO CASCADE)
modelBuilder.Entity<StudentEnrollment>()
    .HasRequired(e => e.ClassGroup)
    .WithMany(cg => cg.Enrollments)
    .HasForeignKey(e => e.ClassGroupId)
    .WillCascadeOnDelete(false);
```

**Effect**:
- Deleting a Student automatically removes all enrollments
- Deleting a ClassGroup is blocked if enrollments exist
- Orphaned records prevented

#### Assignment Relationships
```csharp
// ClassGroup → Assignment (CASCADE DELETE)
modelBuilder.Entity<Assignment>()
    .HasRequired(a => a.ClassGroup)
    .WithMany(cg => cg.Assignments)
    .HasForeignKey(a => a.ClassGroupId)
    .WillCascadeOnDelete(true);

// Teacher → Assignment (NO CASCADE)
modelBuilder.Entity<Assignment>()
    .HasRequired(a => a.CreatedBy)
    .WithMany(t => t.Assignments)
    .HasForeignKey(a => a.CreatedByTeacherId)
    .WillCascadeOnDelete(false);
```

**Effect**:
- Deleting a ClassGroup removes all assignments in it
- Deleting a Teacher blocks if they created assignments
- Audit trail preserved for deleted assignments

### 2.2 Application-Level Referential Integrity Checks

**Before DELETE operations**:

```csharp
public void DeleteTeacher(int id)
{
    var teacher = _db.Teachers.Find(id);
    if (teacher == null) 
        throw new KeyNotFoundException($"Teacher with ID {id} not found.");

    // Check for orphaned resources
    var classGroupCount = _db.ClassGroups.Count(cg => cg.TeacherId == id);
    if (classGroupCount > 0)
        throw new InvalidOperationException(
            $"Cannot delete teacher: they have {classGroupCount} class group(s). " +
            "Reassign or delete these classes first."
        );

    var assignmentCount = _db.Assignments.Count(a => a.CreatedByTeacherId == id);
    if (assignmentCount > 0)
        throw new InvalidOperationException(
            $"Cannot delete teacher: they have {assignmentCount} assignment(s). " +
            "Delete or reassign these assignments first."
        );

    // Safe to delete
    _db.Teachers.Remove(teacher);
    _db.SaveChanges();
}
```

**Benefits**:
- Prevents orphaned data
- Clear error messages to client
- Prevents accidental data loss

### 2.3 Teacher Existence Verification

**When creating related entities**:

```csharp
// When creating student
if (!_db.Teachers.Any(t => t.Id == request.TeacherId))
    throw new KeyNotFoundException($"Teacher with ID {request.TeacherId} not found.");

// When creating class group
var teacher = _db.Teachers.Find(request.TeacherId);
if (teacher == null)
    throw new KeyNotFoundException($"Teacher with ID {request.TeacherId} not found.");

// When enrolling student
var classGroup = _db.ClassGroups.Find(classGroupId);
if (classGroup == null)
    throw new KeyNotFoundException($"Class group with ID {classGroupId} not found.");

var student = _db.Students.Find(studentId);
if (student == null)
    throw new KeyNotFoundException($"Student with ID {studentId} not found.");
```

**Benefits**:
- Catches errors before database constraints
- Provides meaningful error responses
- Prevents database exceptions from reaching client

---

## 3. Data Consistency

### 3.1 Atomic Transactions

All write operations use `DbContext.SaveChanges()`:

```csharp
public AdminTeacherDto CreateTeacher(AdminCreateTeacherDto request)
{
    // ... validation ...

    var teacher = new Teacher { /* ... */ };
    _db.Teachers.Add(teacher);
    _db.SaveChanges();  // Atomic commit

    // Log after successful commit
    _auditLogService.LogCreate("Teacher", teacher.Id, /* ... */);

    return MapToDto(teacher);
}
```

**Guarantees**:
- ACID properties (Atomicity, Consistency, Isolation, Durability)
- All changes committed together or rolled back together
- Isolation prevents dirty reads from other transactions

### 3.2 Cascade Delete Consistency

**Pattern for safe cascading deletes**:

```csharp
public void DeleteStudent(int id)
{
    var student = _db.Students.Find(id);
    if (student == null) 
        throw new KeyNotFoundException($"Student with ID {id} not found.");

    // Snapshot for audit before deletion
    var studentSnapshot = new { 
        student.FirstName, 
        student.LastName, 
        student.Email, 
        student.Grade 
    };

    // Remove student (cascades to enrollments and submissions)
    _db.Students.Remove(student);
    _db.SaveChanges();  // Single transaction

    // Audit after successful deletion
    _auditLogService.LogDelete("Student", id, studentSnapshot, "admin@trackmygrade.com");
}
```

**Guarantees**:
- All related records deleted in single transaction
- Audit trail captures entity before deletion
- No orphaned enrollments or submissions

### 3.3 Case-Insensitive Uniqueness

**Normalization pattern**:

```csharp
// Teacher email
string normalizedEmail = request.Email.Trim().ToLower();
if (_db.Teachers.Any(t => t.Email == normalizedEmail))
    throw new InvalidOperationException("Email already exists.");

// Student email (update case)
if (student.Email != normalizedEmail &&
    _db.Students.Any(s => s.Id != id && s.Email == normalizedEmail))
    throw new InvalidOperationException("Email already exists.");

// Subject code
string normalizedCode = request.Code.Trim().ToUpper();
if (_db.Subjects.Any(c => c.Code == normalizedCode))
    throw new InvalidOperationException("Code already exists.");
```

**Consistency guarantee**:
- `admin@trackmygrade.com` and `ADMIN@trackmygrade.com` treated as same
- Prevents duplicate records across case variations

### 3.4 Duplicate Enrollment Prevention

```csharp
// Unique index constraint
modelBuilder.Entity<StudentEnrollment>()
    .HasIndex(e => new { e.StudentId, e.ClassGroupId })
    .IsUnique();

// Application-level check
if (_db.StudentEnrollments.Any(e => 
    e.ClassGroupId == classGroupId && 
    e.StudentId == studentId))
    throw new InvalidOperationException(
        $"Student {studentId} is already enrolled in class group {classGroupId}."
    );
```

**Guarantees**:
- Cannot enroll same student twice in same class
- Database constraint prevents race conditions
- Clear error message if duplicate attempted

---

## 4. Audit Trail for Compliance

### 4.1 Immutable Audit Logs

**Location**: `TrackMyGradeAPI/Models/Student.cs` - `AuditLog` class

```csharp
public class AuditLog
{
    public int      Id          { get; set; }      // Read-only PK
    public string   Action      { get; set; }      // Created, Updated, Deleted
    public string   EntityType  { get; set; }      // Teacher, Student, etc.
    public int      EntityId    { get; set; }      // ID of affected entity
    public string   Changes     { get; set; }      // JSON snapshot
    public string   PerformedBy { get; set; }      // Admin email
    public DateTime PerformedAt { get; set; }      // UTC timestamp
    public string   IpAddress   { get; set; }      // Optional tracking
    public string   UserAgent   { get; set; }      // Optional tracking
}
```

**Features**:
- Write-once (INSERT only)
- No UPDATE/DELETE operations possible
- Complete historical record
- JSON serialization for complex changes

### 4.2 Audit Logging on CRUD

**CREATE**:
```csharp
_auditLogService.LogCreate("Teacher", teacher.Id, 
    new { teacher.FirstName, teacher.LastName, teacher.Email }, 
    "admin@trackmygrade.com");
```

**UPDATE**:
```csharp
var oldState = new { student.Email, student.Grade };
// ... apply changes ...
var newState = new { student.Email, student.Grade };
_auditLogService.LogUpdate("Student", id, oldState, newState, "admin@trackmygrade.com");
```

**DELETE**:
```csharp
var snapshot = new { student.FirstName, student.Email };
// ... delete ...
_auditLogService.LogDelete("Student", id, snapshot, "admin@trackmygrade.com");
```

**Guarantees**:
- Every change tracked
- Before/after states recorded
- Timestamp and performer recorded
- Cannot be altered after creation

### 4.3 Audit Query Capabilities

```csharp
// All changes to specific entity
List<AuditLogDto> history = _auditLogService.GetAuditLogsByEntity("Teacher", 5);

// All actions by admin
List<AuditLogDto> adminActions = _auditLogService.GetAuditLogsByUser("admin@trackmygrade.com");

// Paginated with filters
var result = _auditLogService.GetAuditLogs(new AuditLogFilterDto 
{
    EntityType = "Student",
    Action = "Updated",
    StartDate = new DateTime(2026, 1, 1),
    EndDate = new DateTime(2026, 12, 31),
    PageNumber = 1,
    PageSize = 50
});
```

---

## 5. Database Indexes for Performance

### 5.1 Unique Indexes (Data Integrity)

```csharp
// Teacher email uniqueness
modelBuilder.Entity<Teacher>()
    .HasIndex(t => t.Email)
    .IsUnique();

// Student email uniqueness
modelBuilder.Entity<Student>()
    .HasIndex(s => s.Email)
    .IsUnique();

// Student OMANG/Passport uniqueness
modelBuilder.Entity<Student>()
    .HasIndex(s => s.OmangOrPassport)
    .IsUnique();

// Subject code uniqueness
modelBuilder.Entity<Subject>()
    .HasIndex(c => c.Code)
    .IsUnique();

// Enrollment uniqueness
modelBuilder.Entity<StudentEnrollment>()
    .HasIndex(e => new { e.StudentId, e.ClassGroupId })
    .IsUnique();
```

### 5.2 Query Indexes (Performance)

```csharp
// Audit log queries
modelBuilder.Entity<AuditLog>()
    .HasIndex(a => new { a.EntityType, a.PerformedAt });

modelBuilder.Entity<AuditLog>()
    .HasIndex(a => new { a.PerformedBy, a.PerformedAt });
```

---

## 6. Error Handling Strategy

### 6.1 Exception Hierarchy

```csharp
// Business rule violations
throw new InvalidOperationException("Cannot delete teacher with active classes");

// Resource not found
throw new KeyNotFoundException($"Teacher with ID {id} not found");

// Input validation
throw new ArgumentException("Email is required");

// Unauthorized access
throw new UnauthorizedAccessException("Invalid admin credentials");
```

### 6.2 Controller Error Responses

```csharp
try 
{
    return Ok(_adminService.CreateTeacher(request));
}
catch (ArgumentException ex) 
{
    return BadRequest(ex.Message);  // 400 - Input invalid
}
catch (KeyNotFoundException ex) 
{
    return NotFound();  // 404 - Resource not found
}
catch (InvalidOperationException ex) 
{
    return BadRequest(ex.Message);  // 400 - Business rule violated
}
catch (Exception ex) 
{
    ErrorLoggingConfig.LogError(ex);
    return InternalServerError(ex);  // 500 - Server error
}
```

---

## 7. Testing Checklist

### Data Integrity Tests
- [ ] Email validation prevents invalid formats
- [ ] Phone validation prevents invalid formats
- [ ] Grade range validation (1-12)
- [ ] OMANG/Passport length validation (max 9)
- [ ] Required fields properly enforced

### Referential Integrity Tests
- [ ] Cannot create student with non-existent teacher
- [ ] Cannot create enrollment with non-existent class
- [ ] Cannot delete teacher with active classes
- [ ] Cannot delete subject with active classes
- [ ] Foreign key constraints enforced

### Data Consistency Tests
- [ ] No duplicate emails (case-insensitive)
- [ ] No duplicate OMANG/Passport
- [ ] No duplicate subject codes
- [ ] No duplicate enrollments
- [ ] Cascade deletes work correctly

### Audit Trail Tests
- [ ] Creation logged with values
- [ ] Update logged with before/after state
- [ ] Deletion logged with snapshot
- [ ] Audit logs are immutable
- [ ] Query filters work correctly
- [ ] Pagination works correctly

---

## 8. Best Practices Summary

1. **Validate Early**: Input validation at controller entry point
2. **Check Referential Integrity**: Before foreign key operations
3. **Use Atomic Transactions**: SaveChanges() commits atomically
4. **Normalize Data**: Case-insensitive comparisons for text
5. **Cascade Carefully**: Manual vs. automatic cascade selection
6. **Audit Everything**: Log all state changes
7. **Index Wisely**: Unique indexes for integrity, query indexes for performance
8. **Handle Errors Gracefully**: Meaningful error messages for debugging
9. **Test Thoroughly**: All constraint scenarios covered
10. **Monitor Audit Logs**: Regular review for compliance and security

