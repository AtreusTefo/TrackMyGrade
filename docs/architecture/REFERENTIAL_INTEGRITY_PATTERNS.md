# Referential Integrity Patterns & Best Practices

## Overview

This guide documents the architectural patterns and best practices for maintaining referential integrity and data consistency in TrackMyGrade. These patterns are enforceable at multiple layers: input validation, business logic, foreign key constraints, and database constraints.

---

## Core Principles

### 1. Defense in Depth
**Multiple layers validate the same constraints independently**:
- Frontend: Validation UX (client-side only, not security)
- DTO/Validator: FluentValidation rules
- Service Layer: Business logic checks
- Database: FK constraints, unique indexes
- Application: Concurrency control (RowVersion)

**Benefit**: If one layer fails, others still protect data integrity

### 2. Fail-Fast
**Errors detected and thrown early in the request lifecycle**:
1. Input validation (before DB queries)
2. FK checks (before INSERT/UPDATE)
3. Authorization checks (before data modification)
4. Transaction rollback on any failure

**Benefit**: Prevents time waste and resource consumption on invalid operations

### 3. Atomic Operations
**All related changes succeed together or rollback together**:
- Assignment creation must succeed entirely or not at all
- Enrollment + audit logging must be atomic
- No orphaned or partial records

**Benefit**: Prevents inconsistent state; enables safe error recovery

### 4. Immutable Audit Trail
**All mutations logged before commit**:
- AuditLog captures intent (old values → new values)
- PerformedAt always UTC
- PerformedBy captured for accountability
- No audit trail modifications allowed

**Benefit**: Full compliance, debugging, and accountability

---

## Pattern 1: Foreign Key Validation

### When to Apply
Every CREATE or UPDATE operation that involves a FK relationship

### Implementation

```csharp
public ClassGroupDto CreateClassGroup(CreateClassGroupDto request)
{
	// 1. Input validation (DTOs/Validators)
	AdminValidator.ValidateCreateClassGroup(request);

	// 2. FK: Verify subject exists and is not soft-deleted
	var subject = _db.Subjects.Find(request.SubjectId);
	if (subject == null || subject.IsDeleted)
		throw new KeyNotFoundException(
			$"Subject with ID {request.SubjectId} not found or deleted.");

	// 3. FK: Verify teacher exists
	var teacher = _db.Teachers.Find(request.TeacherId);
	if (teacher == null)
		throw new KeyNotFoundException(
			$"Teacher with ID {request.TeacherId} not found.");

	// 4. Business logic validation
	// (check for duplicates, state constraints, etc.)

	// 5. Persist with transaction
	using (var transaction = _db.Database.BeginTransaction())
	{
		try
		{
			var group = new ClassGroup { /* ... */ };
			_db.ClassGroups.Add(group);
			_db.SaveChanges();

			// 6. Audit BEFORE commit
			_auditLogService.LogCreate("ClassGroup", group.Id, 
				new { group.Name, group.TeacherId }, "admin@system.com");

			transaction.Commit();
			return MapToDto(group);
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			throw new InvalidOperationException("Failed to create class group.", ex);
		}
	}
}
```

### Common Errors & Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| `DbUpdateException: FK_... violated` | FK validation skipped in service | Add explicit Find() check before Insert |
| `Orphaned records in DB` | No rollback on partial failure | Wrap in transaction with catch-finally |
| `Wrong error message to user` | Generic DB exception thrown | Catch, translate to 409/400 with clear message |

### Testing Pattern

```csharp
[Fact]
public void CreateClassGroup_WithInvalidSubjectId_Throws()
{
	// Arrange
	var request = new CreateClassGroupDto { SubjectId = 9999, /* ... */ };

	// Act & Assert
	var ex = Assert.Throws<KeyNotFoundException>(
		() => _service.CreateClassGroup(request));
	Assert.Contains("Subject", ex.Message);
}
```

---

## Pattern 2: Soft Delete Filtering

### When to Apply
Every retrieval operation that may return deleted records

### Implementation

```csharp
// Pattern: Filter with !entity.IsDeleted in all queries
public List<SubjectDto> GetAllSubjects()
{
	return _db.Subjects
		.Where(s => !s.IsDeleted)  // ← Always filter soft-deleted
		.Select(s => new SubjectDto 
		{ 
			Id = s.Id, 
			Name = s.Name, 
			Code = s.Code 
		})
		.ToList();
}

// For relationship traversal
public List<AdminStudentDto> GetAllStudents()
{
	return _db.Students
		// Note: Students don't have IsDeleted flag, but enrollments do
		.Select(s => new AdminStudentDto { /* ... */ })
		.ToList();
}

// Duplicate checks must respect IsDeleted
if (_db.StudentEnrollments.Any(
	e => e.StudentId == studentId && 
		 e.ClassGroupId == classGroupId && 
		 !e.IsDeleted))  // ← Must check soft-deleted too
	throw new InvalidOperationException("Already enrolled");
```

### Why Not Use Global Filters?

EF6 doesn't support global query filters (EF Core 5+ does). Manual filtering is required:
- **Pro**: Explicit; no surprises; easy to audit
- **Con**: Verbose; easy to forget filter

### Testing Pattern

```csharp
[Fact]
public void GetAllSubjects_ExcludesSoftDeletedRecords()
{
	// Arrange: Create 2 subjects, soft-delete 1
	var subject1 = new Subject { Name = "Active", IsDeleted = false };
	var subject2 = new Subject { Name = "Deleted", IsDeleted = true };
	_db.Subjects.AddRange(subject1, subject2);
	_db.SaveChanges();

	// Act
	var result = _service.GetAllSubjects();

	// Assert
	Assert.Single(result);
	Assert.Equal("Active", result[0].Name);
}
```

---

## Pattern 3: Transaction Management

### When to Apply
Operations with multiple steps that must be atomic

### Implementation

```csharp
// Multi-step operation: Create enrollment + log audit
public void EnrollStudent(int classGroupId, int studentId)
{
	// 1. Validation
	var classGroup = _db.ClassGroups.Find(classGroupId);
	if (classGroup == null || classGroup.IsDeleted)
		throw new KeyNotFoundException("Class group not found");

	var student = _db.Students.Find(studentId);
	if (student == null)
		throw new KeyNotFoundException("Student not found");

	// 2. Transaction: Begin
	using (var transaction = _db.Database.BeginTransaction())
	{
		try
		{
			// 3. Create enrollment
			var enrollment = new StudentEnrollment 
			{ 
				StudentId = studentId, 
				ClassGroupId = classGroupId,
				EnrolledAt = DateTime.UtcNow 
			};
			_db.StudentEnrollments.Add(enrollment);
			_db.SaveChanges();  // ← SaveChanges within transaction

			// 4. Log audit (before commit, to capture intent)
			_auditLogService.LogCreate("StudentEnrollment", enrollment.Id,
				new { enrollment.StudentId, enrollment.ClassGroupId },
				"admin@system.com");

			// 5. Commit
			transaction.Commit();
		}
		catch (Exception ex)
		{
			// 6. Rollback on error
			transaction.Rollback();
			throw new InvalidOperationException(
				"Failed to enroll student. Transaction rolled back.", ex);
		}
	}
}
```

### Common Issues

| Issue | Cause | Fix |
|-------|-------|-----|
| `Transaction timeout` | Long-running operations | Minimize DB calls; use .AsNoTracking() |
| `Deadlocks` | Multiple transactions competing for locks | Order DB operations consistently |
| `Rollback silently fails` | No catch block | Always catch and throw with context |

### Testing Pattern

```csharp
[Fact]
public void EnrollStudent_RollsBackOnAuditLogFailure()
{
	// Arrange
	var classGroup = new ClassGroup { /* ... */ };
	var student = new Student { /* ... */ };
	_db.AddRange(classGroup, student);
	_db.SaveChanges();

	// Mock audit service to throw
	_mockAuditLogService.Setup(x => x.LogCreate(It.IsAny<string>(), It.IsAny<int>(), 
		It.IsAny<object>(), It.IsAny<string>()))
		.Throws<Exception>();

	// Act & Assert
	var ex = Assert.Throws<InvalidOperationException>(
		() => _service.EnrollStudent(classGroup.Id, student.Id));

	// Verify enrollment was NOT created
	Assert.Empty(_db.StudentEnrollments);
}
```

---

## Pattern 4: State Machine Enforcement

### When to Apply
Operations that transition entity state (status, workflow stages)

### Implementation

```csharp
public void GradeSubmission(int submissionId, GradingDto request)
{
	var submission = _db.AssignmentSubmissions.Find(submissionId);
	if (submission == null)
		throw new KeyNotFoundException("Submission not found");

	// 1. Define valid state transitions
	// Pending → Graded (valid)
	// Late → Graded (valid)
	// Graded → Graded (INVALID - no re-grading)

	// 2. Validate current state allows transition
	var validStates = new[] { SubmissionStatus.Pending, SubmissionStatus.Late };
	if (!validStates.Contains(submission.Status))
		throw new InvalidOperationException(
			$"Cannot grade submission in '{submission.Status}' status. " +
			$"Only '{SubmissionStatus.Pending}' or '{SubmissionStatus.Late}' submissions can be graded.");

	// 3. Validate target state is allowed (prevent invalid values)
	if (submission.Status == SubmissionStatus.Graded)
		throw new InvalidOperationException(
			"This submission has already been graded. Re-grading is not permitted.");

	// 4. Perform transition (other validations...)
	submission.Status = SubmissionStatus.Graded;
	submission.Score = request.Score;
	submission.UpdatedAt = DateTime.UtcNow;
	_db.SaveChanges();
}
```

### State Transition Diagram

```
┌─────────────────────────────────────┐
│                                     │
│   ┌──────────┐    ┌──────────┐    │
│   │ Pending  │───→│  Graded  │    │
│   └──────────┘    └──────────┘    │
│        ↓                             │
│   ┌──────────┐                      │
│   │   Late   │───┐                  │
│   └──────────┘   │                  │
│                  ├──→ [Graded]      │
│                  (final state)      │
│                                     │
└─────────────────────────────────────┘

Valid Transitions:
  Pending → Graded ✓
  Late → Graded ✓
  Graded → Graded ✗ (invalid)
  Pending → Late ✗ (no manual override)
  Any → Pending ✗ (no backtrack)
```

### Testing Pattern

```csharp
[Theory]
[InlineData(SubmissionStatus.Pending, true)]
[InlineData(SubmissionStatus.Late, true)]
[InlineData(SubmissionStatus.Graded, false)]
public void GradeSubmission_ValidatesStateTransition(string currentStatus, bool shouldSucceed)
{
	// Arrange
	var submission = new AssignmentSubmission { Status = currentStatus };
	_db.AssignmentSubmissions.Add(submission);
	_db.SaveChanges();

	// Act & Assert
	if (shouldSucceed)
		_service.GradeSubmission(submission.Id, new GradingDto { Score = 85 });
	else
		Assert.Throws<InvalidOperationException>(
			() => _service.GradeSubmission(submission.Id, new GradingDto { Score = 85 }));
}
```

---

## Pattern 5: Audit Trail Logging

### When to Apply
All CREATE, UPDATE, DELETE operations

### Implementation

```csharp
// Pattern: Log BEFORE commit (captures intent)
using (var transaction = _db.Database.BeginTransaction())
{
	try
	{
		// 1. Create entity
		var entity = new Entity { /* ... */ };
		_db.Entities.Add(entity);
		_db.SaveChanges();

		// 2. Log CREATE before commit (captures intent)
		_auditLogService.LogCreate(
			entityType: "EntityName",
			entityId: entity.Id,
			newValues: new { entity.Property1, entity.Property2 },
			performedBy: "user@system.com",
			ipAddress: GetUserIpAddress(),
			userAgent: GetUserAgent()
		);

		// 3. Commit everything together
		transaction.Commit();
	}
	catch (Exception ex)
	{
		transaction.Rollback();
		throw;
	}
}
```

### Update Audit Pattern

```csharp
// For updates: capture old state before modification
var oldState = new { 
	teacher.FirstName, 
	teacher.LastName, 
	teacher.Email 
};

teacher.FirstName = request.FirstName;
teacher.LastName = request.LastName;

_auditLogService.LogUpdate(
	entityType: "Teacher",
	entityId: teacher.Id,
	oldValues: oldState,
	newValues: new { 
		teacher.FirstName, 
		teacher.LastName, 
		teacher.Email 
	},
	performedBy: "admin@system.com"
);
```

### Querying Audit Logs

```csharp
// Get all changes to a specific entity
var logs = _auditLogService.GetAuditLogsByEntity("Student", studentId);

// Get all operations by a user
var userOps = _auditLogService.GetAuditLogsByUser("teacher@school.com");

// Filter by date range
var logs = _auditLogService.GetAuditLogs(new AuditLogFilterDto 
{ 
	StartDate = DateTime.UtcNow.AddDays(-7),
	EndDate = DateTime.UtcNow
});
```

---

## Pattern 6: Concurrency Control

### When to Apply
Entities frequently updated concurrently (grades, submissions)

### Implementation

```csharp
// 1. Add [Timestamp] to entity
public class AssignmentSubmission
{
	public int Id { get; set; }
	public int? Score { get; set; }

	[Timestamp]  // ← SQL Server TIMESTAMP (auto-increments on UPDATE)
	public byte[] RowVersion { get; set; }
}

// 2. Configure in DbContext (automatically done via annotation)
// modelBuilder.Entity<AssignmentSubmission>()
//     .Property(e => e.RowVersion).IsRowVersion();

// 3. Handle concurrency exception
try
{
	submission.Score = request.Score;
	submission.UpdatedAt = DateTime.UtcNow;
	_db.SaveChanges();
}
catch (DbUpdateConcurrencyException ex)
{
	// Another user modified this submission
	var entry = ex.Entries.Single();
	var databaseEntity = entry.GetDatabaseValues();

	return new {
		error = "Concurrency conflict: Another user modified this submission.",
		conflictingEntity = databaseEntity,
		clientVersion = ex.Entries.Single().OriginalValues,
		serverVersion = databaseEntity
	};
}

// 4. Client-side handling: Show dialog, prompt refresh or merge
// Response to UI: 409 Conflict with conflicting values
// UI: Display to user, offer "Refresh & Retry" or "Accept Latest"
```

### Testing Pattern

```csharp
[Fact]
public void GradeSubmission_DetectsConcurrentModification()
{
	// Arrange
	var submission = new AssignmentSubmission { Score = null };
	_db.AssignmentSubmissions.Add(submission);
	_db.SaveChanges();

	// Simulate concurrent update
	var submission2 = _db.AssignmentSubmissions.Find(submission.Id);
	submission2.Score = 90;
	_db.SaveChanges();  // RowVersion incremented by SQL Server

	// Act: Original submission still has old RowVersion
	submission.Score = 85;
	var ex = Assert.Throws<DbUpdateConcurrencyException>(
		() => _db.SaveChanges());

	// Assert
	Assert.NotNull(ex);
}
```

---

## Anti-Patterns to Avoid

### ❌ Anti-Pattern 1: Trust EF6 Cascade Alone

```csharp
// BAD: Relies on EF cascade without checking first
_db.Students.Remove(student);
_db.SaveChanges();  // ← Trusts cascade to delete enrollments

// GOOD: Verify state before cascade
var enrollmentCount = _db.StudentEnrollments.Count(e => e.StudentId == student.Id);
if (enrollmentCount > 0)
	throw new InvalidOperationException($"Cannot delete student: {enrollmentCount} enrollments exist.");
_db.Students.Remove(student);
_db.SaveChanges();
```

### ❌ Anti-Pattern 2: Forgetting Soft Delete Checks

```csharp
// BAD: Returns deleted records
public List<SubjectDto> GetAllSubjects()
{
	return _db.Subjects
		.Select(s => new SubjectDto { /* ... */ })
		.ToList();
}

// GOOD: Excludes deleted records
public List<SubjectDto> GetAllSubjects()
{
	return _db.Subjects
		.Where(s => !s.IsDeleted)  // ← Always filter
		.Select(s => new SubjectDto { /* ... */ })
		.ToList();
}
```

### ❌ Anti-Pattern 3: Skipping FK Validation

```csharp
// BAD: No validation, assumes FK exists
public void EnrollStudent(int studentId, int classGroupId)
{
	var enrollment = new StudentEnrollment 
	{ 
		StudentId = studentId, 
		ClassGroupId = classGroupId 
	};
	_db.StudentEnrollments.Add(enrollment);
	_db.SaveChanges();  // ← May fail with FK constraint violation
}

// GOOD: Validate FK before insert
public void EnrollStudent(int studentId, int classGroupId)
{
	if (!_db.Students.Any(s => s.Id == studentId))
		throw new KeyNotFoundException($"Student {studentId} not found");
	if (!_db.ClassGroups.Any(c => c.Id == classGroupId && !c.IsDeleted))
		throw new KeyNotFoundException($"Class group {classGroupId} not found");

	var enrollment = new StudentEnrollment { /* ... */ };
	_db.StudentEnrollments.Add(enrollment);
	_db.SaveChanges();
}
```

### ❌ Anti-Pattern 4: No Transaction Management

```csharp
// BAD: Multiple SaveChanges() calls (not atomic)
_db.StudentEnrollments.Add(enrollment);
_db.SaveChanges();
_auditLogService.LogCreate(...);  // ← If this fails, enrollment persists

// GOOD: Transaction wraps both
using (var tx = _db.Database.BeginTransaction())
{
	try
	{
		_db.StudentEnrollments.Add(enrollment);
		_db.SaveChanges();
		_auditLogService.LogCreate(...);
		tx.Commit();
	}
	catch
	{
		tx.Rollback();
		throw;
	}
}
```

### ❌ Anti-Pattern 5: Ignoring State Machine Rules

```csharp
// BAD: No state validation
public void GradeSubmission(int submissionId, GradingDto request)
{
	var submission = _db.AssignmentSubmissions.Find(submissionId);
	submission.Score = request.Score;
	submission.Status = SubmissionStatus.Graded;
	_db.SaveChanges();  // ← Can re-grade already graded submissions
}

// GOOD: Validate state transition
if (submission.Status == SubmissionStatus.Graded)
	throw new InvalidOperationException("Already graded; no re-grading allowed");
submission.Score = request.Score;
submission.Status = SubmissionStatus.Graded;
_db.SaveChanges();
```

---

## Checklist for New Features

When adding a new feature involving data persistence:

- [ ] **Input Validation**: Create DTO + FluentValidator
- [ ] **FK Validation**: Service checks all FK references exist before INSERT/UPDATE
- [ ] **Soft Delete**: Queries filter `.Where(e => !e.IsDeleted)` 
- [ ] **Transactions**: Multi-step operations wrapped in `BeginTransaction()`
- [ ] **Audit Logging**: All mutations logged with `_auditLogService`
- [ ] **State Machine** (if applicable): Validate state transitions
- [ ] **Concurrency** (if applicable): Add `[Timestamp]` to frequently updated entities
- [ ] **Error Handling**: Catch DB exceptions, translate to 400/409/500 with meaningful messages
- [ ] **Tests**: Unit tests for FK validation, soft deletes, state transitions
- [ ] **Documentation**: Update REFERENTIAL_INTEGRITY_IMPLEMENTATION.md

---

## Related Files

- **AGENTS.md**: Core guidelines (FK validation, soft deletes, audit trails)
- **ApplicationDbContext.cs**: EF6 FK configuration
- **AssignmentValidator.cs**: Input validation rules
- **AuditLogService.cs**: Audit trail implementation
- **REFERENTIAL_INTEGRITY_IMPLEMENTATION.md**: Implementation summary
- **REFERENTIAL_INTEGRITY_TEST_GUIDE.md**: Test cases and scenarios

