# Data Flow & Integrity Architecture

## Before vs After Comparison

### BEFORE: Admin Creates Student (UNSAFE)
```
Admin UI
  ↓ (submit form - no validation)
  ↓
Angular Component (createStudent)
  ↓ (no checks)
  ↓
HTTP POST /api/admin/students
  ↓
AdminController (no validation)
  ↓
AdminService.CreateStudent
  ├─ Email duplicate check
  ├─ OMANG duplicate check
  └─ Teacher FK check? MISSING!
  ↓
Database
  └─ StudentEnrollment orphaned ✗ if teacher deleted later
```

### AFTER: Admin Creates Student (SAFE)
```
Admin UI
  ↓ (user fills form)
  ↓
Angular Component
  ├─ validateStudentForm()
  │  ├─ Email format check
  │  ├─ Phone format check
  │  ├─ Grade range check (1-12)
  │  ├─ OMANG required check
  │  ├─ Teacher selected check
  │  └─ Display errors per-field
  ├─ Check submitting flag (prevent duplicates)
  └─ Only submit if valid
      ↓
      HTTP POST /api/admin/students
        ↓
        AdminController
        ├─ Try block
        ├─ Call AdminService
        └─ Catch specific exceptions
          ├─ ArgumentException → 400
          ├─ KeyNotFoundException → 400
          └─ Etc.
              ↓
              AdminService.CreateStudent
              ├─ AdminValidator.ValidateCreateStudent(request)
              │  ├─ Email format regex
              │  ├─ Phone format regex
              │  ├─ Name length validation
              │  ├─ Grade range 1-12
              │  ├─ OMANG required
              │  └─ Teacher ID required
              ├─ Check duplicate email (case-insensitive)
              ├─ Check duplicate OMANG
  ├─ Verify teacher exists NEW!
              └─ Insert student
                  ↓
                  Database (safe, all constraints checked)
```

---

## Data Integrity Checks - Layered Approach

```
┌─────────────────────────────────────────────────────────────────┐
│ LAYER 1: CLIENT-SIDE VALIDATION (Angular Component)            │
├─────────────────────────────────────────────────────────────────┤
│ ✓ Email format check                                            │
│ ✓ Phone format check                                            │
│ ✓ Required fields                                               │
│ ✓ Length constraints                                            │
│ ✓ Grade range (1-12)                                            │
│ ✓ Selection dropdowns (no manual entry)                         │
│ Purpose: Immediate user feedback, reduce server load           │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ LAYER 2: API VALIDATION (AdminValidator Static Class)          │
├─────────────────────────────────────────────────────────────────┤
│ ✓ Regex validation (email, phone)                              │
│ ✓ Required field checks                                        │
│ ✓ Length constraints                                           │
│ ✓ Grade range validation                                       │
│ Purpose: Trust but verify before DB operations                │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ LAYER 3: REFERENTIAL INTEGRITY (AdminService)                 │
├─────────────────────────────────────────────────────────────────┤
│ ✓ FK existence checks (teacher, subject, student)              │
│ ✓ Duplicate business key checks (email, OMANG, subject code)   │
│ ✓ Orphaned resource checks (before delete)                    │
│ ✓ Case-insensitive normalization                              │
│ Purpose: Prevent invalid relationships                         │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ LAYER 4: DATABASE CONSTRAINTS (Entity Framework + SQL)         │
├─────────────────────────────────────────────────────────────────┤
│ ✓ Unique indexes (email, OMANG, subject code)                  │
│ ✓ Foreign key constraints                                     │
│ ✓ Unique composite indexes (StudentEnrollment, Submissions)   │
│ ✓ NOT NULL constraints                                        │
│ ✓ Cascade delete rules                                        │
│ Purpose: Last line of defense, data consistency in DB         │
└─────────────────────────────────────────────────────────────────┘
```

---

## Critical Paths - Before & After

### Path 1: Delete Teacher with Active Classes

**BEFORE:**
```
Admin: Delete teacher (John Smith)
  ↓
Delete button → confirm → DELETE /api/admin/teachers/5
  ↓
AdminService.DeleteTeacher(5)
  ├─ Find teacher
  ├─ Remove teacher
  └─ Save ✗ ORPHANED!
      ↓
Database
  ├─ Teacher deleted
  ├─ ClassGroup.TeacherId = 5 (no teacher!)
  └─ StudentEnrollment.ClassGroupId → dead reference
      ↓
App breaks when:
  - Listing classes
  - Showing enrollments
  - Calculating grades
```

**AFTER:**
```
Admin: Delete teacher (John Smith)
  ↓
Delete button → confirm with warning: "This will fail if they have active classes"
  ↓
DELETE /api/admin/teachers/5
  ↓
AdminController
  ├─ Try
  ├─ Call AdminService
  └─ Catch KeyNotFoundException → 404

AdminService.DeleteTeacher(5)
  ├─ Find teacher (id=5, "John Smith")
  ├─ Count ClassGroups where TeacherId=5 → 2
  ├─ Throw InvalidOperationException:
  │  "Cannot delete teacher: they have 2 class group(s).
  │   Reassign or delete these classes first."
  └─ NO DELETE!
      ↓
HTTP 400 Bad Request
  ├─ Message: "Cannot delete teacher: they have 2 class group(s)."
  └─ Admin sees error, takes action

Option 1: Reassign classes to new teacher
Option 2: Delete classes first (cascades to enrollments)
Option 3: Create new teacher record for historical data
```

### Path 2: Create Student with Non-Existent Teacher

**BEFORE:**
```
Admin form: Creates student with teacherId=999 (doesn't exist)
  ↓
POST /api/admin/students
  ↓
AdminService.CreateStudent
  ├─ Check duplicate email ✓
  ├─ Check duplicate OMANG ✓
  └─ INSERT into Students table ✓
      ↓
Database
  ├─ Student created with TeacherId=999
  └─ FK constraint violated? NO (Teacher FK allows non-existing!)
      ↓
App breaks when:
  - Loading student dashboard
  - Querying student's teacher
  - Showing teacher name
```

**AFTER:**
```
Admin form: Tries to create student with teacherId=999
  ↓
POST /api/admin/students (with TeacherId=999)
  ↓
AdminController.CreateStudent
  ├─ Try block
  └─ Call AdminService
      ↓
AdminService.CreateStudent
  ├─ AdminValidator.ValidateCreateStudent(request)
  │  ├─ Email format ✓
  │  ├─ Phone format ✓
  │  ├─ Grade 1-12 ✓
  │  ├─ OMANG required ✓
  │  └─ TeacherId > 0 ✓
  ├─ Check teacher exists:
  │  if (!_db.Teachers.Any(t => t.Id == 999))
  │     throw new KeyNotFoundException("Teacher with ID 999 not found.")
  └─ NOT INSERTED!
      ↓
HTTP 400 Bad Request
  ├─ Message: "Teacher with ID 999 not found."
  └─ Admin must select valid teacher from dropdown
```

### Path 3: Update Student Email to Existing Email

**BEFORE:**
```
Admin editing student: Changes email from "alice@school.com" to "bob@school.com"
(but bob@school.com already exists)
  ↓
PUT /api/admin/students/1 (with email=bob@school.com)
  ↓
AdminService.UpdateStudent
  ├─ Find student
  ├─ Update all fields
  └─ Save ✗ VIOLATED!
      ↓
Database catches duplicate email unique constraint
  └─ Throws ConstraintViolationException
      ↓
Admin sees: "An error occurred" (generic)
Admin confused: What went wrong?
```

**AFTER:**
```
Admin editing student: Changes email from "alice@school.com" to "bob@school.com"
  ↓
PUT /api/admin/students/1
  ↓
AdminService.UpdateStudent
  ├─ AdminValidator.ValidateUpdateStudent(request)
  │  └─ Email format valid ✓
  ├─ Find student (id=1, email="alice@school.com")
  ├─ Check for duplicate email:
  │  if (student.Email != "bob@school.com" &&
  │      _db.Students.Any(s => s.Id != 1 && s.Email == "bob@school.com"))
  │    throw new InvalidOperationException(
  │      "A student with this email already exists.")
  └─ NOT UPDATED!
      ↓
HTTP 400 Bad Request
  ├─ Message: "A student with this email already exists."
  └─ Admin sees clear error, tries different email
```

### Path 4: Duplicate Student Enrollment

**BEFORE:**
```
Admin enrolls student in class:
Click "Enroll" → Student 5 enrolled in Class 3 ✓

(Accidental double-click)
Click "Enroll" again → Student 5 enrolled AGAIN ✗
  ↓
StudentEnrollment table
  ├─ Row 1: StudentId=5, ClassGroupId=3, EnrolledAt=12:00
  ├─ Row 2: StudentId=5, ClassGroupId=3, EnrolledAt=12:00 (DUPLICATE!)
  └─ Unique constraint: VIOLATED!
      ↓
Database error
UI shows: "Error" (generic)
Admin confused
```

**AFTER:**
```
Admin enrolls student:
Click "Enroll" → submitting=true (button disabled)
  ↓
POST /api/admin/class-groups/3/enroll { studentId: 5 }
  ↓
AdminService.EnrollStudent(3, 5)
  ├─ Verify class group exists ✓
  ├─ Verify student exists ✓
  ├─ Check if already enrolled:
  │  if (_db.StudentEnrollments.Any(
  │      e => e.ClassGroupId==3 && e.StudentId==5))
  │    throw InvalidOperationException(
  │      "Student 5 is already enrolled in class group 3.")
  └─ NOT ENROLLED!
      ↓
HTTP 400 Bad Request
  ├─ Message: "Student 5 is already enrolled in class group 3."
  └─ submitting=false (button re-enabled)

(If user double-clicks during request)
  ├─ First request processes
  ├─ second request still in flight
  └─ Database unique index prevents duplicate anyway
```

---

## Cascade Delete Flow

### When Student is Deleted

```
DELETE /api/admin/students/42
  ↓
AdminService.DeleteStudent(42)
  ├─ Find student
  └─ _db.Students.Remove(student)
      ↓
      Entity Framework (OnModelCreating)
        └─ StudentEnrollment.HasRequired(e => e.Student)
           .WithMany(s => s.Enrollments)
           .HasForeignKey(e => e.StudentId)
           .WillCascadeOnDelete(true)  ← KEY!

      Database CASCADE DELETE:
        ├─ DELETE FROM StudentEnrollments WHERE StudentId=42
        │  └─ Removes all enrollments for student 42
        │     (triggering any enrollment-dependent cleanup)
        │
        ├─ DELETE FROM AssignmentSubmissions WHERE StudentId=42
        │  └─ Removes all submissions from student 42
        │     (keeping assignment records for grade history)
        │
        └─ DELETE FROM Students WHERE Id=42
           └─ Student deleted

Result: ✅ Clean removal, no orphans
```

### When Subject is Deleted

```
DELETE /api/admin/subjects/10
  ↓
NO DELETE endpoint in Admin!

(Would need: PUT /api/admin/subjects/{id}/reassign)
```

---

## Summary of Safety Improvements

| Operation | Before | After | Layer |
|-----------|--------|-------|-------|
| Create teacher | Dup email check | Dup email check + format + length | Validator |
| Create student | Dup email + OMANG check | ✓ + Teacher FK check + format + grade | Service |
| Create subject | Dup code check | ✓ + format + length | Validator |
| Create class | Subject/Teacher exist check | ✓ + more validation | Service |
| Update student | Dup checks on create only | ✓ + Dup checks on update | Service |
| Delete teacher | No checks | ✓ Check for orphaned classes | Service |
| Enroll student | Dup check only | ✓ + FK checks + better error | Service |
| UI validation | None | ✓ Client-side checks | Component |
| Error messages | Generic | Specific with IDs & context | Controller |

---

## Prevention Matrix

```
                  │ Client │ Validator │ Service │ Database │
──────────────────┼────────┼───────────┼─────────┼──────────┤
Invalid email     │   ✓    │     ✓     │    ✓    │    ✓     │
Invalid phone     │   ✓    │     ✓     │         │          │
Invalid grade     │   ✓    │     ✓     │         │          │
Dup email         │        │     ✓     │    ✓    │    ✓     │
Dup OMANG         │        │     ✓     │    ✓    │    ✓     │
Dup subject code   │        │     ✓     │    ✓    │    ✓     │
Invalid FK        │        │           │    ✓    │    ✓     │
Orphaned records  │        │           │    ✓    │    ✓     │
Dup enrollment    │        │           │    ✓    │    ✓     │
Bad grade range   │   ✓    │     ✓     │         │          │

Legend:
✓ = Prevented by this layer
(empty) = Relies on previous layers
```

Multiple layers mean:
1. Fast feedback at each stage
2. Prevention at source where possible
3. Safety net at DB if needed
4. No single point of failure
