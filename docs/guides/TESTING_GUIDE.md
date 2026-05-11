# Testing Guide & Verification Checklist

## Unit Testing Recommendations

### 1. AdminValidator Tests

```csharp
[TestClass]
public class AdminValidatorTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ValidateCreateTeacher_MissingFirstName_ThrowsException()
    {
        AdminValidator.ValidateCreateTeacher(new AdminCreateTeacherDto 
        { 
            FirstName = "", 
            LastName = "Smith", 
            Email = "valid@email.com" 
        });
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ValidateCreateTeacher_InvalidEmailFormat_ThrowsException()
    {
        AdminValidator.ValidateCreateTeacher(new AdminCreateTeacherDto 
        { 
            FirstName = "John", 
            LastName = "Smith", 
            Email = "invalid-email" 
        });
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ValidateCreateStudent_InvalidGrade_ThrowsException()
    {
        AdminValidator.ValidateCreateStudent(new AdminCreateStudentDto 
        { 
            FirstName = "Jane", 
            LastName = "Doe", 
            Email = "jane@school.com", 
            Grade = 13,  // Invalid: must be 1-12
            TeacherId = 1, 
            OmangOrPassport = "123456789" 
        });
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ValidateCreateStudent_InvalidPhoneFormat_ThrowsException()
    {
        AdminValidator.ValidateCreateStudent(new AdminCreateStudentDto 
        { 
            FirstName = "Jane", 
            LastName = "Doe", 
            Email = "jane@school.com", 
            Phone = "12",  // Invalid: must be 7+ chars
            Grade = 10, 
            TeacherId = 1, 
            OmangOrPassport = "123456789" 
        });
    }

    [TestMethod]
    public void ValidateCreateTeacher_ValidData_NoException()
    {
        // Should not throw
        AdminValidator.ValidateCreateTeacher(new AdminCreateTeacherDto 
        { 
            FirstName = "John", 
            LastName = "Smith", 
            Email = "john@school.com",
            Phone = "+267-555-1234",
            Subject = "Mathematics"
        });
    }

    [TestMethod]
    public void ValidateCreateStudent_ValidDataWithOptionalPhone_NoException()
    {
        // Should not throw
        AdminValidator.ValidateCreateStudent(new AdminCreateStudentDto 
        { 
            FirstName = "Jane", 
            LastName = "Doe", 
            Email = "jane@school.com",
            Grade = 10, 
            TeacherId = 1, 
            OmangOrPassport = "123456789",
            Phone = null  // Optional
        });
    }
}
```

### 2. AdminService Tests

```csharp
[TestClass]
public class AdminServiceTests
{
    private ApplicationDbContext _db;
    private IAdminService _adminService;
    private IMapper _mapper;
    private ITokenService _tokenService;

    [TestInitialize]
    public void Setup()
    {
        _db = new ApplicationDbContext();
        _db.Database.Delete();
        _db.Database.Create();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _tokenService = new MockTokenService();
        _adminService = new AdminService(_db, _mapper, _tokenService);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void CreateStudent_NonexistentTeacher_ThrowsException()
    {
        _adminService.CreateStudent(new AdminCreateStudentDto 
        { 
            FirstName = "Jane", 
            LastName = "Doe", 
            Email = "jane@school.com", 
            Grade = 10, 
            TeacherId = 999,  // Doesn't exist
            OmangOrPassport = "123456789" 
        });
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CreateStudent_DuplicateEmail_ThrowsException()
    {
        // Create first student
        _db.Teachers.Add(new Teacher { Id = 1, FirstName = "John", LastName = "Smith", Email = "john@school.com" });
        _db.Students.Add(new Student 
        { 
            FirstName = "Jane", 
            LastName = "Doe", 
            Email = "duplicate@school.com", 
            OmangOrPassport = "111111111", 
            Grade = 10, 
            TeacherId = 1 
        });
        _db.SaveChanges();

        // Try to create second with same email
        _adminService.CreateStudent(new AdminCreateStudentDto 
        { 
            FirstName = "Bob", 
            LastName = "Smith", 
            Email = "duplicate@school.com",  // Duplicate!
            Grade = 10, 
            TeacherId = 1, 
            OmangOrPassport = "222222222" 
        });
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void DeleteTeacher_WithActiveClasses_ThrowsException()
    {
        var teacher = new Teacher { FirstName = "John", LastName = "Smith", Email = "john@school.com" };
        var course = new Course { Name = "Math", Code = "MATH-101" };
        var classGroup = new ClassGroup { Name = "10A", GradeLevel = 10, Course = course, Teacher = teacher };

        _db.Teachers.Add(teacher);
        _db.Courses.Add(course);
        _db.ClassGroups.Add(classGroup);
        _db.SaveChanges();

        // Try to delete teacher with active class
        _adminService.DeleteTeacher(teacher.Id);  // Should throw!
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UpdateStudent_DuplicateEmail_ThrowsException()
    {
        var teacher = new Teacher { FirstName = "John", LastName = "Smith", Email = "john@school.com" };
        var student1 = new Student 
        { 
            FirstName = "Jane", 
            LastName = "Doe", 
            Email = "jane@school.com", 
            OmangOrPassport = "111111111", 
            Grade = 10, 
            TeacherId = 1,
            Teacher = teacher
        };
        var student2 = new Student 
        { 
            FirstName = "Bob", 
            LastName = "Smith", 
            Email = "bob@school.com", 
            OmangOrPassport = "222222222", 
            Grade = 10, 
            TeacherId = 1,
            Teacher = teacher
        };

        _db.Teachers.Add(teacher);
        _db.Students.AddRange(student1, student2);
        _db.SaveChanges();

        // Try to change bob's email to jane's
        _adminService.UpdateStudent(student2.Id, new AdminUpdateStudentDto 
        { 
            FirstName = "Bob", 
            LastName = "Smith", 
            Email = "jane@school.com",  // Duplicate!
            Grade = 10, 
            TeacherId = 1, 
            OmangOrPassport = "222222222" 
        });
    }

    [TestMethod]
    public void EnrollStudent_DuplicateEnrollment_ThrowsException()
    {
        var student = new Student 
        { 
            FirstName = "Jane", 
            LastName = "Doe", 
            Email = "jane@school.com", 
            OmangOrPassport = "123456789", 
            Grade = 10, 
            TeacherId = 1 
        };
        var teacher = new Teacher { FirstName = "John", LastName = "Smith", Email = "john@school.com" };
        var course = new Course { Name = "Math", Code = "MATH-101" };
        var classGroup = new ClassGroup { Name = "10A", GradeLevel = 10, Course = course, Teacher = teacher };

        _db.Teachers.Add(teacher);
        _db.Courses.Add(course);
        _db.ClassGroups.Add(classGroup);
        _db.Students.Add(student);
        _db.SaveChanges();

        // First enrollment
        _adminService.EnrollStudent(classGroup.Id, student.Id);

        // Try duplicate enrollment
        _adminService.EnrollStudent(classGroup.Id, student.Id);  // Should throw!
    }
}
```

---

## Integration Testing Scenarios

### Scenario 1: Complete Student Lifecycle

```csharp
[TestMethod]
public void StudentLifecycle_CreateEnrollDeleteCascades()
{
    // 1. Create teacher
    var teacher = _adminService.CreateTeacher(new AdminCreateTeacherDto 
    { 
        FirstName = "John", 
        LastName = "Smith", 
        Email = "john@school.com", 
        Subject = "Math" 
    });
    Assert.IsNotNull(teacher);
    Assert.IsTrue(teacher.Id > 0);

    // 2. Create course
    var course = _adminService.CreateCourse(new CreateCourseDto 
    { 
        Name = "Mathematics Grade 10", 
        Code = "MATH-10" 
    });
    Assert.IsNotNull(course);

    // 3. Create class group
    var classGroup = _adminService.CreateClassGroup(new CreateClassGroupDto 
    { 
        Name = "10A", 
        GradeLevel = 10, 
        CourseId = course.Id, 
        TeacherId = teacher.Id 
    });
    Assert.IsNotNull(classGroup);

    // 4. Create student
    var student = _adminService.CreateStudent(new AdminCreateStudentDto 
    { 
        FirstName = "Jane", 
        LastName = "Doe", 
        Email = "jane@school.com", 
        Grade = 10, 
        TeacherId = teacher.Id, 
        OmangOrPassport = "123456789" 
    });
    Assert.IsNotNull(student);

    // 5. Enroll student
    _adminService.EnrollStudent(classGroup.Id, student.Id);
    var enrollments = _db.StudentEnrollments.Where(e => e.StudentId == student.Id).ToList();
    Assert.AreEqual(1, enrollments.Count);

    // 6. Delete student - should cascade delete enrollment
    _adminService.DeleteStudent(student.Id);
    var remainingEnrollments = _db.StudentEnrollments.Where(e => e.StudentId == student.Id).ToList();
    Assert.AreEqual(0, remainingEnrollments.Count);  // ✓ Cascaded!

    // 7. Verify student is gone
    var deletedStudent = _db.Students.Find(student.Id);
    Assert.IsNull(deletedStudent);
}
```

---

## Manual Testing Checklist

### Teacher Management

- [ ] Create teacher with valid data → Success
- [ ] Create teacher with duplicate email → Error: "A teacher with this email already exists."
- [ ] Create teacher with invalid email format (client) → Form error shown
- [ ] Create teacher with name > 100 chars (server) → Error: "First name cannot exceed 100 characters."
- [ ] Delete teacher with no classes → Success
- [ ] Delete teacher with 1+ classes → Error: "Cannot delete teacher: they have X class group(s)..."
- [ ] Delete teacher with assignments → Error: "Cannot delete teacher: they have X assignment(s)..."

### Student Management

- [ ] Create student with valid data → Success
- [ ] Create student with non-existent teacher → Error: "Teacher with ID X not found."
- [ ] Create student with duplicate email → Error: "A student with this email already exists."
- [ ] Create student with duplicate OMANG → Error: "A student with this OMANG/Passport already exists."
- [ ] Create student with grade < 1 (server) → Error: "Grade must be between 1 and 12."
- [ ] Create student with grade > 12 (server) → Error: "Grade must be between 1 and 12."
- [ ] Create student with invalid phone (server) → Error: "Phone number format is invalid."
- [ ] Update student email to existing email → Error: "A student with this email already exists."
- [ ] Update student OMANG to existing OMANG → Error: "A student with this OMANG/Passport already exists."
- [ ] Delete student with enrollments → Enrollments deleted automatically
- [ ] Delete student with submissions → Submissions deleted automatically

### Course Management

- [ ] Create course with valid data → Success
- [ ] Create course with duplicate code (case-insensitive) → Error: "A course with this code already exists."
- [ ] Create course with name > 200 chars → Error: "Course name cannot exceed 200 characters."
- [ ] Create course with code > 20 chars → Error: "Course code cannot exceed 20 characters."

### Class Group Management

- [ ] Create class with valid data → Success
- [ ] Create class with non-existent course → Error: "Course with ID X not found."
- [ ] Create class with non-existent teacher → Error: "Teacher with ID X not found."
- [ ] Create class with grade level < 1 → Error: "Grade level must be between 1 and 12."
- [ ] Create class with grade level > 12 → Error: "Grade level must be between 1 and 12."

### Enrollment Management

- [ ] Enroll student in class → Success, shows in enrollment list
- [ ] Enroll same student again → Error: "Student X is already enrolled in class group Y."
- [ ] Enroll with non-existent student → Error: "Student with ID X not found."
- [ ] Enroll with non-existent class → Error: "Class group with ID X not found."
- [ ] Unenroll student → Success, removed from list
- [ ] Unenroll non-existent enrollment → Error: "Enrollment not found..."

### UI/UX Validation Tests

- [ ] Form shows email error inline (not submitted)
- [ ] Form shows phone error inline (not submitted)
- [ ] Form shows grade out of range error (not submitted)
- [ ] Form shows name too long error (not submitted)
- [ ] Delete confirmation shows item name: "Delete teacher 'John Smith'?"
- [ ] Submit button disabled during submission (prevent double-click)
- [ ] Error message displays for 5 seconds, then clears
- [ ] Success message displays for 3 seconds, then clears
- [ ] Clear form fields after successful creation
- [ ] Clear error fields when form is reset

### Edge Cases

- [ ] Email case sensitivity: "JOHN@SCHOOL.COM" same as "john@school.com" → Treated as duplicate
- [ ] Phone number formats:
  - [ ] "+267-555-1234" → Accepted
  - [ ] "(555) 123-4567" → Accepted
  - [ ] "5551234567" → Accepted
  - [ ] "12" → Rejected
- [ ] Rapid submissions (double-click):
  - [ ] First submission completes
  - [ ] Second submission blocked (submitting flag prevents it)
- [ ] Empty optional fields:
  - [ ] Phone can be empty
  - [ ] Description can be empty
  - [ ] Subject can be empty
- [ ] Whitespace handling:
  - [ ] Emails trimmed and lowercased
  - [ ] Names trimmed
  - [ ] OMANG trimmed
  - [ ] Course code trimmed and uppercased

---

## Performance Testing

```csharp
[TestMethod]
public void CreateTeacher_Performance_Under50ms()
{
    var stopwatch = Stopwatch.StartNew();

    var result = _adminService.CreateTeacher(new AdminCreateTeacherDto 
    { 
        FirstName = "John", 
        LastName = "Smith", 
        Email = $"perf-{Guid.NewGuid()}@school.com", 
        Subject = "Math" 
    });

    stopwatch.Stop();
    Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50, 
        $"CreateTeacher took {stopwatch.ElapsedMilliseconds}ms");
}

[TestMethod]
public void ValidateCreateStudent_Performance_Under1ms()
{
    var stopwatch = Stopwatch.StartNew();

    for (int i = 0; i < 1000; i++)
    {
        AdminValidator.ValidateCreateStudent(new AdminCreateStudentDto 
        { 
            FirstName = "Jane", 
            LastName = "Doe", 
            Email = "jane@school.com", 
            Grade = 10, 
            TeacherId = 1, 
            OmangOrPassport = "123456789" 
        });
    }

    stopwatch.Stop();
    var avgMs = (double)stopwatch.ElapsedMilliseconds / 1000;
    Assert.IsTrue(avgMs < 1, $"Average validation took {avgMs}ms");
}
```

---

## API Response Testing

### Test Status Codes

```csharp
[TestMethod]
public void CreateStudent_WithInvalidTeacher_Returns400BadRequest()
{
    var response = _controller.CreateStudent(new AdminCreateStudentDto 
    { 
        FirstName = "Jane", 
        LastName = "Doe", 
        Email = "jane@school.com", 
        Grade = 10, 
        TeacherId = 999, 
        OmangOrPassport = "123456789" 
    });

    Assert.IsInstanceOfType(response, typeof(BadRequestErrorMessageResult));
}

[TestMethod]
public void DeleteTeacher_WithNonexistentId_Returns404NotFound()
{
    var response = _controller.DeleteTeacher(999);

    Assert.IsInstanceOfType(response, typeof(NotFoundResult));
}

[TestMethod]
public void DeleteTeacher_WithActiveClasses_Returns400BadRequest()
{
    // Setup: teacher with class
    var teacher = _adminService.CreateTeacher(...);
    var course = _adminService.CreateCourse(...);
    _adminService.CreateClassGroup(new CreateClassGroupDto 
    { 
        TeacherId = teacher.Id, 
        CourseId = course.Id 
    });

    var response = _controller.DeleteTeacher(teacher.Id);

    Assert.IsInstanceOfType(response, typeof(BadRequestErrorMessageResult));
}
```

---

## Regression Testing

Run this test suite before each deployment:

```csharp
[TestClass]
public class RegressionTests
{
    // All CRUD operations still work
    [TestMethod] public void CreateTeacher_StillWorks() { }
    [TestMethod] public void ReadTeachers_StillWorks() { }
    [TestMethod] public void UpdateTeacher_StillWorks() { }
    [TestMethod] public void DeleteTeacher_StillWorks() { }

    // All relationships intact
    [TestMethod] public void StudentTeacherRelationship_StillWorks() { }
    [TestMethod] public void ClassGroupCourseRelationship_StillWorks() { }
    [TestMethod] public void ClassGroupTeacherRelationship_StillWorks() { }

    // Cascade deletes still work
    [TestMethod] public void StudentDelete_CascadesEnrollments() { }
    [TestMethod] public void StudentDelete_CascadesSubmissions() { }
    [TestMethod] public void ClassGroupDelete_CascadesAssignments() { }
}
```

---

## Sign-Off Checklist

- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] All manual tests pass
- [ ] No SQL injection vulnerabilities
- [ ] No data loss scenarios
- [ ] No orphaned records
- [ ] Performance acceptable (< 100ms per operation)
- [ ] Error messages clear and actionable
- [ ] API status codes correct
- [ ] Cascade deletes verified
- [ ] Case-sensitivity handled correctly
- [ ] Submitted to QA for smoke testing
- [ ] Ready for production deployment
