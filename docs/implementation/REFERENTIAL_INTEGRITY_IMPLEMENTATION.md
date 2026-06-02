# Referential Integrity & Data Consistency - Implementation Summary

## Overview

This document summarizes comprehensive improvements made to referential integrity and data consistency across the TrackMyGrade application. All changes follow AGENTS.md standards and enforce data integrity at multiple layers: input validation (DTOs), business logic (service layer), foreign key constraints (EF6), and database constraints (SQL Server).

---

## Key Improvements Implemented

### 1. Optimistic Concurrency Control

**Added RowVersion (Timestamp) Columns**
- StudentEnrollment, Assignment, AssignmentSubmission now include [Timestamp] RowVersion fields
- SQL Server TIMESTAMP columns automatically increment on every UPDATE
- EF6 detects concurrent modifications and throws DbUpdateConcurrencyException
- Migration: 202505270001_AddConcurrencyRowVersionColumns

**Benefit**: Prevents last-write-wins scenarios; ensures concurrent operations don't corrupt data

---

### 2. Foreign Key Validation at Service Layer

**AssignmentService**
- CreateAssignment validates ClassGroupId and CreatedByTeacherId exist before persisting
- SubmitAssignment validates AssignmentId, StudentId exist and student is enrolled
- GradeSubmission validates SubmissionId, AssignmentId exist with authorization check
- All FK validation occurs BEFORE SaveChanges() to fail-fast on constraint violations

**AdminService**
- CreateClassGroup validates SubjectId (non-deleted) and TeacherId exist
- EnrollStudent validates ClassGroupId (non-deleted) and StudentId exist
- Duplicate enrollment checks respect IsDeleted flag to prevent re-enrolling deleted students
- CreateStudent validates TeacherId exists

**StudentService**
- GetByTeacher and GetById check active enrollments (excludes soft-deleted)

**Pattern**: All FK checks throw KeyNotFoundException with entity ID for clear error messages

---

### 3. Soft Delete Query Filters

**Updated Query Methods to Exclude IsDeleted=true Records**
- AssignmentService: GetMyAssignments, GetStudentAssignments, GetSubmissions, GetMySubmissions
- AdminService: GetAllSubjects, GetAllClassGroups
- StudentService: GetByTeacher, GetById

**Auditable Deletion**: When soft-deleting, IsDeleted=true but record remains in database for audit trail

**Benefit**: Prevents data leakage; historical records preserved for compliance; false positives avoided

---

### 4. Transaction Management

**Multi-Step Operations Wrapped in Transactions**
- AssignmentService: CreateAssignment, SubmitAssignment, GradeSubmission
- AdminService: EnrollStudent (enrollment + audit logging combined)

**Transaction Pattern**:
```csharp
using (var transaction = _db.Database.BeginTransaction())
{
	try
	{
		// Business logic and SaveChanges()
		transaction.Commit();
	}
	catch (Exception ex)
	{
		transaction.Rollback();
		throw new InvalidOperationException("Operation failed. Transaction rolled back.", ex);
	}
}
```

**Benefit**: If any step fails, entire transaction rolls back; prevents orphaned records; ensures atomicity

---

### 5. Submission Status State Machine

**Enforced State Transitions in GradeSubmission**
- Valid transitions: (Pending OR Late) → Graded
- Invalid transitions: Graded → Graded (prevents re-grading)
- Rejects invalid status values with clear error message

**Example**:
```csharp
if (submission.Status == SubmissionStatus.Graded)
	throw new InvalidOperationException(
		"This submission has already been graded. Re-grading is not permitted.");
```

**Benefit**: Prevents data corruption from invalid state transitions; maintains business logic constraints

---

### 6. Comprehensive Input Validation

**Created AssignmentValidator**
- AssignmentCreateValidator: Validates title, description, dueDate, maxScore, classGroupId
- SubmissionCreateValidator: Validates submission content (required, 1-2000 chars)
- GradingValidator: Validates score (0-100) and feedback (0-2000 chars)

**Pattern**: Uses FluentValidation with Cascade(CascadeMode.Stop) for fail-fast

**Benefit**: Prevents malformed data entry; consistent validation across all entry points

---

### 7. Timestamp Synchronization

**All Timestamps Use DateTime.UtcNow**
- Models: CreatedAt, UpdatedAt, EnrolledAt, SubmittedAt
- AuditLog: PerformedAt uses DateTime.UtcNow
- Database: SQL Server datetime2 type (supports UTC timestamps)

**Validation**: No timestamp can be in future (except DueDate which is validated explicitly)

**Benefit**: No timezone ambiguity; audit trail is accurate; historical data queryable by date

---

### 8. Audit Trail Enforcement

**All Create/Update/Delete Operations Logged**
- AuditLogService.LogCreate: Captures entity type, ID, new values
- AuditLogService.LogUpdate: Captures entity type, ID, old → new values
- AuditLogService.LogDelete: Captures entity type, ID, old values
- Logging occurs BEFORE SaveChanges() in transactions (captures intent)

**Benefit**: Full compliance audit trail; can track data changes; admin oversight enabled

---

## Referential Integrity Guarantees

### FK Constraints Configured in DbContext

**One-to-Many Relationships**
- Teacher → ClassGroups: NO cascade (manual check in DeleteTeacher)
- Teacher → Assignments: NO cascade (manual check in DeleteTeacher)
- Subject → ClassGroups: NO cascade (manual check in DeleteSubject)
- ClassGroup → StudentEnrollments: CASCADE DELETE
- ClassGroup → Assignments: CASCADE DELETE
- Student → StudentEnrollments: CASCADE DELETE
- Student → Submissions: CASCADE DELETE
- Assignment → Submissions: CASCADE DELETE

**Composite Uniqueness Constraints**
- (StudentId, ClassGroupId) unique: No duplicate enrollments in same class
- (SubjectId, Name) unique: No duplicate class names within same subject

**Benefit**: Database enforces constraints at write-time; orphaned records prevented

---

## Data Integrity Workflow

### Create Operation (Example: CreateAssignment)
1. **Input Validation**: Check title, dueDate, maxScore constraints
2. **FK Validation**: Verify ClassGroupId and CreatedByTeacherId exist
3. **Authorization**: Check teacher owns the class group
4. **Business Logic**: Validate DueDate > now, MaxScore > 0
5. **Transaction**: Begin transaction
6. **Persist**: Insert assignment row
7. **Audit**: Log CREATE before commit
8. **Commit**: Transaction commits
9. **Response**: Return DTO with ID

### Failure Points (Fail-Fast)
- Step 1 fails: Return 400 Bad Request (validation error)
- Step 2 fails: Return 409 Conflict (FK not found)
- Step 3 fails: Return 403 Forbidden (authorization denied)
- Step 5 fails: Rollback transaction, return 500 with message

### Success Criteria
- Assignment row inserted
- AuditLog row inserted
- Both in same transaction; either both commit or both rollback
- Response includes new assignment ID

---

## Testing Checklist

### Unit Tests (Per Service)
- [ ] FK validation: FK not found throws KeyNotFoundException
- [ ] Soft delete: Deleted records excluded from queries
- [ ] State machine: Invalid transitions throw InvalidOperationException
- [ ] Transactions: Rollback on exception

### Integration Tests
- [ ] Create assignment: FK valid → success; FK invalid → 409 Conflict
- [ ] Submit assignment: Student not enrolled → 403 Forbidden
- [ ] Grade submission: Already graded → 400 Bad Request
- [ ] Enroll student: Student already enrolled → 400 Bad Request
- [ ] Delete class group: Has enrollments → 400 Bad Request

### Data Consistency Tests
- [ ] Soft delete: IsDeleted=true but record exists; queries exclude it
- [ ] Cascade delete: Delete class → all enrollments deleted; all assignments deleted
- [ ] Audit trail: Every operation logged; old/new values captured

### Concurrency Tests
- [ ] Two grade operations on same submission: Second throws DbUpdateConcurrencyException
- [ ] Two enrollment operations on same student/class: Second throws duplicate error

---

## Error Codes & Messages

### FK Validation
```
409 Conflict
"Student with ID 123 not found."
"Teacher with ID 456 not found."
"Class group with ID 789 not found or has been deleted."
```

### Authorization
```
403 Forbidden
"You can only create assignments for your own classes."
"You did not create this assignment; access denied."
"Student not found or not enrolled in your class."
```

### State Machine
```
400 Bad Request
"This submission has already been graded. Re-grading is not permitted."
"Invalid submission status 'Unknown' for grading. Only 'Pending' or 'Late' can be graded."
```

### Soft Delete
```
404 Not Found
"Class group with ID 100 not found or has been deleted."
```

---

## Performance Considerations

### Query Optimization
- All .Any() and .Find() calls use indexed FK columns
- Soft delete filters use boolean index on IsDeleted column
- Consider caching teacher/subject lists for high-traffic reads

### Transaction Overhead
- Transactions hold locks; keep operations short
- Consider async logging in future (currently synchronous)

### Concurrency Handling
- DbUpdateConcurrencyException requires frontend retry logic
- Users should refresh stale data before attempting updates

---

## Migration Path

### Required Actions
1. Run EF6 migration: 202505270001_AddConcurrencyRowVersionColumns
2. Deploy updated services with FK validation and soft delete filters
3. Existing data: RowVersion TIMESTAMP columns auto-populate on first UPDATE

### Backward Compatibility
- Existing records gain RowVersion on first update (auto-populated by SQL Server)
- Soft delete queries exclude deleted records; no data loss
- FK validation is additive; doesn't break existing valid data

---

## Related Documentation

- **AGENTS.md**: Core development standards (FK validation, soft deletes, audit trails)
- **ApplicationDbContext.cs**: EF6 FK relationship configuration
- **AssignmentValidator.cs**: Input validation rules
- **AuditLogService.cs**: Audit trail implementation

---

## Future Enhancements

1. **Add Concurrency Conflict Resolution**: UI to show conflicting version + allow merge/refresh
2. **Async Audit Logging**: Fire-and-forget logging to prevent blocking on high-volume operations
3. **Database Constraints Migration**: Move some service-layer checks to SQL Check constraints
4. **Query Performance Monitoring**: Log slow queries; identify N+1 patterns
5. **Rate Limiting**: Prevent bulk operations that could cause transaction timeouts

