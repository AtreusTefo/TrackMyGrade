# TrackMyGrade - Data Integrity & Multi-Relationship Architecture

**Status:** ANALYSIS & RECOMMENDATIONS  
**Date:** 2026-06-02  
**Phase:** Post-Validation Implementation

---

## Current State

### What's Working
✅ Single-direction relationships implemented
- One Teacher → Many Students (via StudentTeacherId FK)
- One Subject → Many ClassGroups (via ClassGroupSubjectId FK)
- One ClassGroup → Many Students (via StudentEnrollment)

✅ Validation implemented for single relationships
- Teacher must exist before creating student
- Subject must exist before creating class group
- All input validation enforced at 3 layers

✅ Audit logging for all mutations
- Create/Update/Delete tracked
- User context captured
- Timestamp recorded

---

## Current Limitation: No True Many-to-Many Support

### Problem Statement
The admin dashboard currently supports:
- One teacher per student (hard-coded in Student entity)
- One subject per teacher (string field, not relationship)
- One teacher per class group (hard-coded FK)

**Users Need:**
- A **teacher** to teach **multiple subjects** and **multiple grades**
- A **student** to be taught by **multiple teachers** and **study multiple subjects**
- A **class group** to be taught by **multiple teachers** and taught in **multiple subjects**

### Why This Matters
1. **Real-world scenario**: Dr. Smith teaches both Math AND Physics to multiple grades
2. **Curriculum requirement**: A class group needs 2+ teachers (lead + assistant)
3. **Student flexibility**: A student takes assignments from different teachers

---

## Database Schema Changes Required

### New Junction Tables

#### 1. TeacherSubject (Teacher ↔ Subject)
```sql
CREATE TABLE TeacherSubjects (
	Id INT PRIMARY KEY IDENTITY(1,1),
	TeacherId INT NOT NULL FOREIGN KEY REFERENCES Teachers(Id) ON DELETE CASCADE,
	SubjectId INT NOT NULL FOREIGN KEY REFERENCES Subjects(Id) ON DELETE CASCADE,
	AssignedDate DATETIME2 DEFAULT GETUTCDATE(),
	UNIQUE (TeacherId, SubjectId)
);
```

**Purpose:** Break "Subject" string field in Teacher; enable teacher to teach multiple subjects

#### 2. StudentTeacher (Student ↔ Teacher)
```sql
CREATE TABLE StudentTeachers (
	Id INT PRIMARY KEY IDENTITY(1,1),
	StudentId INT NOT NULL FOREIGN KEY REFERENCES Students(Id) ON DELETE CASCADE,
	TeacherId INT NOT NULL FOREIGN KEY REFERENCES Teachers(Id) ON DELETE CASCADE,
	EnrollmentDate DATETIME2 DEFAULT GETUTCDATE(),
	UNIQUE (StudentId, TeacherId)
);
```

**Purpose:** Break hard-coded TeacherId in Student; enable student to have multiple teachers

#### 3. ClassGroupTeacher (ClassGroup ↔ Teacher)
```sql
CREATE TABLE ClassGroupTeachers (
	Id INT PRIMARY KEY IDENTITY(1,1),
	ClassGroupId INT NOT NULL FOREIGN KEY REFERENCES ClassGroups(Id) ON DELETE CASCADE,
	TeacherId INT NOT NULL FOREIGN KEY REFERENCES Teachers(Id) ON DELETE CASCADE,
	Role VARCHAR(50), -- 'Lead', 'Assistant', 'Guest'
	EnrollmentDate DATETIME2 DEFAULT GETUTCDATE(),
	UNIQUE (ClassGroupId, TeacherId)
);
```

**Purpose:** Break hard-coded TeacherId in ClassGroup; enable class group to have multiple teachers

#### 4. ClassGroupSubject (ClassGroup ↔ Subject)
```sql
CREATE TABLE ClassGroupSubjects (
	Id INT PRIMARY KEY IDENTITY(1,1),
	ClassGroupId INT NOT NULL FOREIGN KEY REFERENCES ClassGroups(Id) ON DELETE CASCADE,
	SubjectId INT NOT NULL FOREIGN KEY REFERENCES Subjects(Id) ON DELETE CASCADE,
	EnrollmentDate DATETIME2 DEFAULT GETUTCDATE(),
	UNIQUE (ClassGroupId, SubjectId)
);
```

**Purpose:** Break hard-coded SubjectId in ClassGroup; enable class group to teach multiple subjects

---

## Entity Model Changes

### Before (Current - Single Relationships)
```csharp
public class Teacher
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Subject { get; set; }  // ❌ String, not relationship
	// ...
}

public class Student
{
	public int Id { get; set; }
	public int TeacherId { get; set; }  // ❌ Hard-coded single teacher
	public Teacher Teacher { get; set; }
	// ...
}

public class ClassGroup
{
	public int Id { get; set; }
	public int SubjectId { get; set; }  // ✅ OK - relationship exists
	public int TeacherId { get; set; }  // ❌ Hard-coded single teacher
	public Teacher Teacher { get; set; }
	// ...
}
```

### After (Proposed - Many-to-Many)
```csharp
public class Teacher
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	// ✅ Remove Subject string field

	// ✅ Add navigation collections
	public ICollection<TeacherSubject> TeacherSubjects { get; set; }
	public ICollection<StudentTeacher> StudentTeachers { get; set; }
	public ICollection<ClassGroupTeacher> ClassGroupTeachers { get; set; }
}

public class Student
{
	public int Id { get; set; }
	// ❌ Remove TeacherId FK - move to StudentTeacher junction
	public Teacher Teacher { get; set; }  // ❌ Remove navigation

	// ✅ Add new navigation collection
	public ICollection<StudentTeacher> StudentTeachers { get; set; }
}

public class ClassGroup
{
	public int Id { get; set; }
	public int SubjectId { get; set; }  // ❌ Remove - move to ClassGroupSubject
	public int TeacherId { get; set; }  // ❌ Remove - move to ClassGroupTeacher

	// ✅ Add navigation collections
	public ICollection<ClassGroupTeacher> ClassGroupTeachers { get; set; }
	public ICollection<ClassGroupSubject> ClassGroupSubjects { get; set; }
}

// ✅ New junction entities
public class TeacherSubject
{
	public int Id { get; set; }
	public int TeacherId { get; set; }
	public int SubjectId { get; set; }
	public DateTime AssignedDate { get; set; }

	public Teacher Teacher { get; set; }
	public Subject Subject { get; set; }
}

public class StudentTeacher
{
	public int Id { get; set; }
	public int StudentId { get; set; }
	public int TeacherId { get; set; }
	public DateTime EnrollmentDate { get; set; }

	public Student Student { get; set; }
	public Teacher Teacher { get; set; }
}

public class ClassGroupTeacher
{
	public int Id { get; set; }
	public int ClassGroupId { get; set; }
	public int TeacherId { get; set; }
	public string Role { get; set; }
	public DateTime EnrollmentDate { get; set; }

	public ClassGroup ClassGroup { get; set; }
	public Teacher Teacher { get; set; }
}

public class ClassGroupSubject
{
	public int Id { get; set; }
	public int ClassGroupId { get; set; }
	public int SubjectId { get; set; }
	public DateTime EnrollmentDate { get; set; }

	public ClassGroup ClassGroup { get; set; }
	public Subject Subject { get; set; }
}
```

---

## DTO Changes Required

### New DTOs for Many-to-Many

```csharp
// ── Teacher Management ──

public class TeacherWithSubjectsDto
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string Phone { get; set; }
	public List<SubjectDto> Subjects { get; set; }  // ✅ Array instead of string
	public bool IsActivated { get; set; }
}

// ── Student Management ──

public class StudentWithTeachersDto
{
	public int Id { get; set; }
	public string StudentNumber { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string Phone { get; set; }
	public string OmangOrPassport { get; set; }
	public int Grade { get; set; }
	public List<TeacherDto> Teachers { get; set; }  // ✅ Array instead of single
	public List<SubjectDto> Subjects { get; set; }  // ✅ New: subjects student takes
	public bool IsActivated { get; set; }
}

// ── Class Group Management ──

public class ClassGroupWithTeachersDto
{
	public int Id { get; set; }
	public string Name { get; set; }
	public int GradeLevel { get; set; }
	public List<SubjectDto> Subjects { get; set; }  // ✅ Array instead of single
	public List<ClassGroupTeacherDto> Teachers { get; set; }  // ✅ Array with roles
	public List<StudentDto> Students { get; set; }
}

public class ClassGroupTeacherDto
{
	public int TeacherId { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string Role { get; set; }  // 'Lead', 'Assistant', 'Guest'
}

// ── Creation Requests ──

public class CreateTeacherWithSubjectsDto
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string Phone { get; set; }
	public List<int> SubjectIds { get; set; }  // ✅ Array of subject IDs
}

public class CreateStudentWithTeachersDto
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string Phone { get; set; }
	public string OmangOrPassport { get; set; }
	public int Grade { get; set; }
	public List<int> TeacherIds { get; set; }  // ✅ Array of teacher IDs
	public List<int> SubjectIds { get; set; }  // ✅ Array of subject IDs
}
```

---

## API Endpoint Changes

### Existing Endpoints (To Remove/Deprecate)
```
POST   /api/admin/teachers          → Replace with new payload
POST   /api/admin/students          → Replace with new payload
POST   /api/admin/classgroups       → Replace with new payload
```

### New Endpoints (To Add)
```
POST   /api/admin/teachers/{id}/subjects      → Assign subject to teacher
DELETE /api/admin/teachers/{id}/subjects/{id} → Remove subject from teacher

POST   /api/admin/students/{id}/teachers      → Assign teacher to student
DELETE /api/admin/students/{id}/teachers/{id} → Remove teacher from student

POST   /api/admin/classgroups/{id}/teachers   → Assign teacher to class
DELETE /api/admin/classgroups/{id}/teachers/{id} → Remove teacher from class

POST   /api/admin/classgroups/{id}/subjects   → Assign subject to class
DELETE /api/admin/classgroups/{id}/subjects/{id} → Remove subject from class
```

---

## Frontend Form Changes

### Teacher Form Update
```html
<!-- Before: Single select -->
<select name="subject">
  <option>Mathematics</option>
  <option>Science</option>
</select>

<!-- After: Multi-select -->
<select name="subjects" multiple>
  <option>Mathematics</option>
  <option>Science</option>
  <option>History</option>
</select>
```

### Student Form Update
```html
<!-- Before: Single select -->
<select name="teacherId">
  <option>Dr. Smith</option>
  <option>Dr. Jones</option>
</select>

<!-- After: Multi-select -->
<select name="teacherIds" multiple>
  <option>Dr. Smith</option>
  <option>Dr. Jones</option>
  <option>Ms. Brown</option>
</select>

<!-- New: Subject selection -->
<select name="subjectIds" multiple>
  <option>Mathematics</option>
  <option>Science</option>
  <option>English</option>
</select>
```

### Class Group Form Update
```html
<!-- Before: Single teacher, single subject -->
<select name="teacherId">
  <option>Dr. Smith</option>
</select>
<select name="subjectId">
  <option>Mathematics</option>
</select>

<!-- After: Multi-teacher, multi-subject -->
<select name="teacherIds" multiple>
  <option>Dr. Smith (Lead)</option>
  <option>Ms. Brown (Assistant)</option>
</select>
<select name="subjectIds" multiple>
  <option>Mathematics</option>
  <option>Statistics</option>
</select>
```

---

## Migration Strategy (Recommended)

### Phase 1: Data Preparation (2-3 days)
1. Create migration script to create 4 new junction tables
2. Seed TeacherSubject data from existing Subject strings
3. Seed StudentTeacher data from existing TeacherId FKs
4. Seed ClassGroupTeacher data from existing TeacherId FKs
5. Seed ClassGroupSubject data from existing SubjectId FKs

### Phase 2: Code Changes (5-7 days)
1. Update EF6 DbContext OnModelCreating() for new tables
2. Update entity models with new relationships
3. Update AutoMapper configuration
4. Create new DTOs
5. Update services (TeacherService, StudentService, etc.)
6. Update controllers with new endpoints

### Phase 3: Frontend Updates (3-5 days)
1. Update admin-dashboard component
2. Add multi-select UI components
3. Update validation for arrays
4. Test all CRUD operations
5. Update audit logging for junction tables

### Phase 4: Testing & Rollout (3-4 days)
1. Integration testing
2. End-to-end testing
3. Performance testing (indexes on FKs)
4. User acceptance testing
5. Gradual rollout

**Total Estimated Time:** 13-19 days

---

## Validation Changes for Many-to-Many

### New Validation Rules

```csharp
// Validate teacher has at least 1 subject assigned
if (request.SubjectIds == null || request.SubjectIds.Count == 0)
	throw new ArgumentException("Teacher must teach at least one subject.");

// Validate all subject IDs exist
var invalidSubjects = request.SubjectIds
	.Where(id => !_db.Subjects.Any(s => s.Id == id))
	.ToList();
if (invalidSubjects.Any())
	throw new ArgumentException($"Invalid subject IDs: {string.Join(", ", invalidSubjects)}");

// Validate student has at least 1 teacher assigned
if (request.TeacherIds == null || request.TeacherIds.Count == 0)
	throw new ArgumentException("Student must have at least one teacher assigned.");

// Validate all teacher IDs exist
var invalidTeachers = request.TeacherIds
	.Where(id => !_db.Teachers.Any(t => t.Id == id))
	.ToList();
if (invalidTeachers.Any())
	throw new ArgumentException($"Invalid teacher IDs: {string.Join(", ", invalidTeachers)}");

// Validate no duplicate assignments
var duplicateTeachers = request.TeacherIds.GroupBy(id => id)
	.Where(g => g.Count() > 1)
	.Select(g => g.Key)
	.ToList();
if (duplicateTeachers.Any())
	throw new ArgumentException($"Duplicate teacher IDs: {string.Join(", ", duplicateTeachers)}");
```

---

## Audit Logging for Junction Tables

### Add Records
```csharp
_auditLogService.LogCreate("TeacherSubject", 
	junction.Id, 
	new { junction.TeacherId, junction.SubjectId }, 
	currentUser);
```

### Remove Records
```csharp
_auditLogService.LogDelete("TeacherSubject", 
	junction.Id, 
	new { junction.TeacherId, junction.SubjectId }, 
	currentUser);
```

---

## Risk Assessment

### High Risk
- Existing students/teachers with hard-coded FKs must be migrated
- Breaking API change (payload structure)
- Frontend forms must support multi-select
- Database migration requires downtime

### Medium Risk
- Audit log volume increases (more junction records)
- Query performance (may need indexes)
- Circular dependencies (teach A → B → teach A)

### Mitigation
- Comprehensive testing before rollout
- Gradual migration (keep both schemas temporarily)
- Index creation on all FK columns
- Duplicate prevention at junction table level

---

## Recommendation

**Implement in Next Sprint:** Assign 2 developers for 2-3 weeks

**Dependencies:**
1. This validation fix (✅ COMPLETE)
2. Database migration scripts
3. EF6 DbContext updates
4. Service layer refactoring
5. Frontend component enhancements

**Success Criteria:**
- ✅ Teachers can teach multiple subjects
- ✅ Students can be taught by multiple teachers
- ✅ Class groups can be taught by multiple teachers/subjects
- ✅ All audit logs track junction table changes
- ✅ No data loss during migration
- ✅ Zero-downtime deployment possible

---

## Conclusion

The current validation fixes address immediate form input and constraint issues. To fully support the real-world requirement of many-to-many relationships, a 2-3 week database and API refactoring project is required.

**Current State (Post This Fix):** ✅ STABLE
**Next State (Many-to-Many):** 📋 PLANNED

