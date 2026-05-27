using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BCrypt.Net;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;
using TrackMyGradeAPI.Validators;

namespace TrackMyGradeAPI.Services
{
    /// <summary>Service interface for administrative operations: teachers, students, subjects, and class groups.</summary>
    public interface IAdminService
    {
        /// <summary>Authenticates an admin user and returns a JWT token.</summary>
        /// <param name="request">The admin login request.</param>
        /// <returns>The admin response with profile and token.</returns>
        AdminResponseDto Login(AdminLoginDto request);

        /// <summary>Gets all teacher accounts.</summary>
        /// <returns>A list of all teacher DTOs.</returns>
        List<AdminTeacherDto>  GetAllTeachers();
        /// <summary>Creates a new teacher account.</summary>
        /// <param name="request">The teacher creation request.</param>
        /// <returns>The created teacher DTO.</returns>
        AdminTeacherDto        CreateTeacher(AdminCreateTeacherDto request);
        /// <summary>Deletes a teacher account.</summary>
        /// <param name="id">The teacher ID to delete.</param>
        void                   DeleteTeacher(int id);

        /// <summary>Gets all student accounts.</summary>
        /// <returns>A list of all student DTOs.</returns>
        List<AdminStudentDto>  GetAllStudents();
        /// <summary>Creates a new student account.</summary>
        /// <param name="request">The student creation request.</param>
        /// <returns>The created student DTO.</returns>
        AdminStudentDto        CreateStudent(AdminCreateStudentDto request);
        /// <summary>Updates a student's personal details.</summary>
        /// <param name="id">The student ID to update.</param>
        /// <param name="request">The student update request.</param>
        /// <returns>The updated student DTO.</returns>
        AdminStudentDto        UpdateStudent(int id, AdminUpdateStudentDto request);
        /// <summary>Deletes a student account.</summary>
        /// <param name="id">The student ID to delete.</param>
        void                   DeleteStudent(int id);

        /// <summary>Gets all subjects.</summary>
        /// <returns>A list of all subject DTOs.</returns>
        List<SubjectDto>        GetAllSubjects();
        /// <summary>Creates a new subject.</summary>
        /// <param name="request">The subject creation request.</param>
        /// <returns>The created subject DTO.</returns>
        SubjectDto              CreateSubject(CreateSubjectDto request);
        /// <summary>Deletes a subject.</summary>
        /// <param name="id">The subject ID to delete.</param>
        void                    DeleteSubject(int id);

        /// <summary>Gets all class groups.</summary>
        /// <returns>A list of all class group DTOs.</returns>
        List<ClassGroupDto>    GetAllClassGroups();
        /// <summary>Creates a new class group.</summary>
        /// <param name="request">The class group creation request.</param>
        /// <returns>The created class group DTO.</returns>
        ClassGroupDto          CreateClassGroup(CreateClassGroupDto request);
        /// <summary>Deletes a class group.</summary>
        /// <param name="id">The class group ID to delete.</param>
        void                   DeleteClassGroup(int id);
        /// <summary>Enrolls a student in a class group.</summary>
        /// <param name="classGroupId">The class group ID.</param>
        /// <param name="studentId">The student ID to enroll.</param>
        /// <returns>The updated class group DTO.</returns>
        ClassGroupDto          EnrollStudent(int classGroupId, int studentId);
        /// <summary>Unenrolls a student from a class group.</summary>
        /// <param name="classGroupId">The class group ID.</param>
        /// <param name="studentId">The student ID to unenroll.</param>
        void                   UnenrollStudent(int classGroupId, int studentId);
    }

    /// <summary>Service implementation for administrative operations.</summary>
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper              _mapper;
        private readonly ITokenService        _tokenService;
        private readonly IAuditLogService     _auditLogService;

        /// <summary>Initializes a new instance of the AdminService class.</summary>
        /// <param name="db">The application database context.</param>
        /// <param name="mapper">The AutoMapper instance.</param>
        /// <param name="tokenService">The token service dependency.</param>
        /// <param name="auditLogService">The audit log service dependency.</param>
        public AdminService(ApplicationDbContext db, IMapper mapper, ITokenService tokenService, IAuditLogService auditLogService)
        {
            _db                = db;
            _mapper            = mapper;
            _tokenService      = tokenService;
            _auditLogService   = auditLogService;
        }

        // ── Auth ──────────────────────────────────────────────────────────

        /// <summary>
        /// Authenticates an admin with email and password, returning a JWT token.
        /// </summary>
        /// <param name="request">The admin login credentials.</param>
        /// <returns>The admin response DTO with JWT token.</returns>
        public AdminResponseDto Login(AdminLoginDto request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    throw new ArgumentException("Email and password are required.");

                // Normalize email for case-insensitive lookup
                string normalizedEmail = request.Email.Trim().ToLower();

                System.Diagnostics.Debug.WriteLine($"[AdminService] Login attempt for email: {normalizedEmail}");

                // Query database for admin account
                var admin = _db.Admins.FirstOrDefault(a => a.Email == normalizedEmail);

                if (admin == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminService] No admin found with email: {normalizedEmail}");
                    System.Diagnostics.Debug.WriteLine($"[AdminService] Total admins in database: {_db.Admins.Count()}");
                    throw new UnauthorizedAccessException("Invalid admin credentials.");
                }

                System.Diagnostics.Debug.WriteLine($"[AdminService] Admin found: {admin.Email}, checking password...");

                // Verify password using BCrypt
                bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, admin.Password);

                if (!passwordValid)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminService] Password verification failed for admin: {admin.Email}");
                    throw new UnauthorizedAccessException("Invalid admin credentials.");
                }

                System.Diagnostics.Debug.WriteLine($"[AdminService] Password verified successfully for admin: {admin.Email}");

                var token = _tokenService.GenerateToken(admin.Id, "Admin", admin.Email);
                System.Diagnostics.Debug.WriteLine($"[AdminService] Login successful. Token issued for: {admin.Email}");

                return new AdminResponseDto
                {
                    Id = admin.Id,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Email = admin.Email,
                    Phone = admin.Phone,
                    Token = token
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminService] Unexpected error during login: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[AdminService] Exception: {ex.GetType().Name} - {ex.StackTrace}");
                throw;
            }
        }

        // ── Teachers ──────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves a list of all teachers in the system.
        /// </summary>
        /// <returns>List of teacher DTOs.</returns>
        public List<AdminTeacherDto> GetAllTeachers()
        {
            return _db.Teachers
                .Select(t => new AdminTeacherDto
                {
                    Id              = t.Id,
                    FirstName       = t.FirstName,
                    LastName        = t.LastName,
                    Email           = t.Email,
                    Phone           = t.Phone,
                    Subject         = t.Subject,
                    IsActivated     = t.IsActivated,
                    ActivationToken = t.IsActivated ? null : t.ActivationToken,
                    ActivatedAt     = t.ActivatedAt
                }).ToList();
        }

        /// <summary>
        /// Creates a new teacher account with the provided information.
        /// </summary>
        /// <param name="request">The teacher creation DTO.</param>
        /// <returns>The created teacher response DTO.</returns>
        public AdminTeacherDto CreateTeacher(AdminCreateTeacherDto request)
        {
            // ── Validate input ─────────────────────────────────────────────
            AdminValidator.ValidateCreateTeacher(request);

            // ── Check email uniqueness (case-insensitive) ──────────────────
            string normalizedEmail = request.Email.Trim().ToLower();
            if (_db.Teachers.Any(t => t.Email == normalizedEmail))
                throw new InvalidOperationException("A teacher with this email already exists.");

            var teacher = new Teacher
            {
                FirstName       = request.FirstName.Trim(),
                LastName        = request.LastName.Trim(),
                Email           = normalizedEmail,
                Phone           = request.Phone?.Trim(),
                Subject         = request.Subject?.Trim(),
                IsActivated     = false,
                ActivationToken = Guid.NewGuid().ToString("N"),
                PasswordHash    = null
            };

            _db.Teachers.Add(teacher);
            _db.SaveChanges();

            _auditLogService.LogCreate("Teacher", teacher.Id, new { teacher.FirstName, teacher.LastName, teacher.Email, teacher.Subject }, "admin@trackmygrade.com");

            return new AdminTeacherDto
            {
                Id = teacher.Id, FirstName = teacher.FirstName, LastName = teacher.LastName,
                Email = teacher.Email, Phone = teacher.Phone, Subject = teacher.Subject,
                IsActivated = false, ActivationToken = teacher.ActivationToken
            };
        }

        /// <summary>
        /// Deletes a teacher account. Only succeeds if teacher has no assigned classes or assignments.
        /// </summary>
        /// <param name="id">The ID of the teacher to delete.</param>
        public void DeleteTeacher(int id)
        {
            var teacher = _db.Teachers.Find(id);
            if (teacher == null) throw new KeyNotFoundException($"Teacher with ID {id} not found.");

            // ── Check for orphaned class groups (referential integrity) ─────
            var classGroupCount = _db.ClassGroups.Count(cg => cg.TeacherId == id);
            if (classGroupCount > 0)
                throw new InvalidOperationException(
                    $"Cannot delete teacher: they have {classGroupCount} class group(s). " +
                    "Reassign or delete these classes first."
                );

            // ── Check for assignments that might be affected ────────────────
            var assignmentCount = _db.Assignments.Count(a => a.CreatedByTeacherId == id);
            if (assignmentCount > 0)
                throw new InvalidOperationException(
                    $"Cannot delete teacher: they have {assignmentCount} assignment(s). " +
                    "Delete or reassign these assignments first."
                );

            // ── Safe to delete ────────────────────────────────────────────
            var teacherSnapshot = new { teacher.FirstName, teacher.LastName, teacher.Email, teacher.Subject };
            _db.Teachers.Remove(teacher);
            _db.SaveChanges();

            _auditLogService.LogDelete("Teacher", id, teacherSnapshot, "admin@trackmygrade.com");
        }

        // ── Students ──────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves a list of all students in the system.
        /// </summary>
        /// <returns>List of student DTOs.</returns>
        public List<AdminStudentDto> GetAllStudents()
        {
            return _db.Students
                .Select(s => new AdminStudentDto
                {
                    Id = s.Id, StudentNumber = s.StudentNumber,
                    FirstName = s.FirstName, LastName = s.LastName,
                    Email = s.Email, Phone = s.Phone,
                    OmangOrPassport = s.OmangOrPassport, Grade = s.Grade,
                    IsActivated = s.IsActivated,
                    ActivationToken = s.IsActivated ? null : s.ActivationToken,
                    ActivatedAt = s.ActivatedAt, TeacherId = s.TeacherId
                }).ToList();
        }

        /// <summary>
        /// Creates a new student account with the provided information.
        /// </summary>
        /// <param name="request">The student creation DTO.</param>
        /// <returns>The created student response DTO.</returns>
        public AdminStudentDto CreateStudent(AdminCreateStudentDto request)
        {
            // ── Validate input ─────────────────────────────────────────────
            AdminValidator.ValidateCreateStudent(request);

            // ── Verify teacher exists (referential integrity) ───────────────
            if (!_db.Teachers.Any(t => t.Id == request.TeacherId))
                throw new KeyNotFoundException($"Teacher with ID {request.TeacherId} not found.");

            // ── Check for duplicate email (case-insensitive) ────────────────
            string normalizedEmail = request.Email.Trim().ToLower();
            if (_db.Students.Any(s => s.Email == normalizedEmail))
                throw new InvalidOperationException("A student with this email already exists.");

            // ── Check for duplicate OMANG/Passport ─────────────────────────
            string normalizedPassport = request.OmangOrPassport.Trim();
            if (_db.Students.Any(s => s.OmangOrPassport == normalizedPassport))
                throw new InvalidOperationException("A student with this OMANG/Passport already exists.");

            var student = new Student
            {
                StudentNumber   = StudentService.GenerateStudentNumber(_db),
                FirstName       = request.FirstName.Trim(),
                LastName        = request.LastName.Trim(),
                Email           = normalizedEmail,
                Phone           = request.Phone?.Trim(),
                OmangOrPassport = normalizedPassport,
                Grade           = request.Grade,
                TeacherId       = request.TeacherId,
                IsActivated     = false,
                ActivationToken = Guid.NewGuid().ToString("N"),
                PasswordHash    = null
            };

            _db.Students.Add(student);
            _db.SaveChanges();

            _auditLogService.LogCreate("Student", student.Id, 
                new { student.FirstName, student.LastName, student.Email, student.Grade, student.TeacherId }, 
                "admin@trackmygrade.com");

            return new AdminStudentDto
            {
                Id = student.Id, StudentNumber = student.StudentNumber,
                FirstName = student.FirstName, LastName = student.LastName,
                Email = student.Email, Phone = student.Phone,
                OmangOrPassport = student.OmangOrPassport, Grade = student.Grade,
                TeacherId = student.TeacherId, IsActivated = false,
                ActivationToken = student.ActivationToken
            };
        }

        /// <summary>
        /// Updates an existing student account with new information.
        /// </summary>
        /// <param name="id">The ID of the student to update.</param>
        /// <param name="request">The student update DTO.</param>
        /// <returns>The updated student response DTO.</returns>
        public AdminStudentDto UpdateStudent(int id, AdminUpdateStudentDto request)
        {
            // ── Validate input ─────────────────────────────────────────────
            AdminValidator.ValidateUpdateStudent(request);

            var student = _db.Students.Find(id);
            if (student == null) throw new KeyNotFoundException($"Student with ID {id} not found.");

            // ── Verify teacher exists (referential integrity) ───────────────
            if (!_db.Teachers.Any(t => t.Id == request.TeacherId))
                throw new KeyNotFoundException($"Teacher with ID {request.TeacherId} not found.");

            // ── Check for duplicate email (excluding current student) ───────
            string normalizedEmail = request.Email.Trim().ToLower();
            if (student.Email != normalizedEmail &&
                _db.Students.Any(s => s.Id != id && s.Email == normalizedEmail))
                throw new InvalidOperationException("A student with this email already exists.");

            // ── Check for duplicate OMANG/Passport (excluding current student) ──
            string normalizedPassport = request.OmangOrPassport.Trim();
            if (student.OmangOrPassport != normalizedPassport &&
                _db.Students.Any(s => s.Id != id && s.OmangOrPassport == normalizedPassport))
                throw new InvalidOperationException("A student with this OMANG/Passport already exists.");

            // ── Capture old state for audit ────────────────────────────────
            var oldState = new { student.FirstName, student.LastName, student.Email, student.Grade, student.TeacherId };

            // ── Update student ─────────────────────────────────────────────
            student.FirstName       = request.FirstName.Trim();
            student.LastName        = request.LastName.Trim();
            student.Email           = normalizedEmail;
            student.Phone           = request.Phone?.Trim();
            student.OmangOrPassport = normalizedPassport;
            student.Grade           = request.Grade;
            student.TeacherId       = request.TeacherId;
            _db.SaveChanges();

            var newState = new { student.FirstName, student.LastName, student.Email, student.Grade, student.TeacherId };
            _auditLogService.LogUpdate("Student", id, oldState, newState, "admin@trackmygrade.com");

            return new AdminStudentDto
            {
                Id = student.Id, StudentNumber = student.StudentNumber,
                FirstName = student.FirstName, LastName = student.LastName,
                Email = student.Email, Phone = student.Phone,
                OmangOrPassport = student.OmangOrPassport, Grade = student.Grade,
                TeacherId = student.TeacherId, IsActivated = student.IsActivated
            };
        }

        /// <summary>
        /// Deletes a student account. Cascades delete related enrollments and submissions.
        /// </summary>
        /// <param name="id">The ID of the student to delete.</param>
        public void DeleteStudent(int id)
        {
            var student = _db.Students.Find(id);
            if (student == null) throw new KeyNotFoundException($"Student with ID {id} not found.");

            var studentSnapshot = new { student.FirstName, student.LastName, student.Email, student.Grade };

            // ── StudentEnrollments cascade delete is configured in DbContext ─
            // ── AssignmentSubmissions cascade delete is configured in DbContext ──
            _db.Students.Remove(student);
            _db.SaveChanges();

            _auditLogService.LogDelete("Student", id, studentSnapshot, "admin@trackmygrade.com");
        }

        // ── Subjects ───────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves a list of all subjects in the system.
        /// </summary>
        /// <returns>List of subject DTOs.</returns>
        public List<SubjectDto> GetAllSubjects()
        {
            return _db.Subjects
                .Select(c => new SubjectDto { Id = c.Id, Name = c.Name, Code = c.Code, Description = c.Description })
                .ToList();
        }

        /// <summary>
        /// Creates a new subject in the system.
        /// </summary>
        /// <param name="request">The subject creation DTO.</param>
        /// <returns>The created subject DTO.</returns>
        public SubjectDto CreateSubject(CreateSubjectDto request)
        {
            // ── Validate input ─────────────────────────────────────────────
            AdminValidator.ValidateCreateSubject(request);

            // ── Check for duplicate subject code (case-insensitive) ─────────
            string normalizedCode = request.Code.Trim().ToUpper();
            if (_db.Subjects.Any(c => c.Code == normalizedCode))
                throw new InvalidOperationException("A subject with this code already exists.");

            var subject = new Subject
            {
                Name = request.Name.Trim(),
                Code = normalizedCode,
                Description = request.Description?.Trim()
            };
            _db.Subjects.Add(subject);
            _db.SaveChanges();

            _auditLogService.LogCreate("Subject", subject.Id, new { subject.Name, subject.Code, subject.Description }, "admin@trackmygrade.com");

            return new SubjectDto { Id = subject.Id, Name = subject.Name, Code = subject.Code, Description = subject.Description };
        }

        /// <summary>
        /// Deletes a subject. Only succeeds if subject has no assigned classes.
        /// </summary>
        /// <param name="id">The ID of the subject to delete.</param>
        public void DeleteSubject(int id)
        {
            var subject = _db.Subjects.Find(id);
            if (subject == null) throw new KeyNotFoundException($"Subject with ID {id} not found.");

            var classGroupCount = _db.ClassGroups.Count(cg => cg.SubjectId == id);
            if (classGroupCount > 0)
                throw new InvalidOperationException($"Cannot delete subject: {classGroupCount} class group(s) are assigned to it.");

            var subjectSnapshot = new { subject.Name, subject.Code, subject.Description };
            _db.Subjects.Remove(subject);
            _db.SaveChanges();

            _auditLogService.LogDelete("Subject", id, subjectSnapshot, "admin@trackmygrade.com");
        }

        // ── Class Groups ──────────────────────────────────────────────────

        /// <summary>
        /// Retrieves a list of all class groups in the system.
        /// </summary>
        /// <returns>List of class group DTOs.</returns>
        public List<ClassGroupDto> GetAllClassGroups()
        {
            return _db.ClassGroups
                .Select(cg => new ClassGroupDto
                {
                    Id = cg.Id, Name = cg.Name, GradeLevel = cg.GradeLevel,
                    SubjectId = cg.SubjectId, SubjectName = cg.Subject.Name,
                    TeacherId = cg.TeacherId,
                    TeacherName = cg.Teacher.FirstName + " " + cg.Teacher.LastName
                }).ToList();
        }

        /// <summary>
        /// Creates a new class group with the provided information.
        /// </summary>
        /// <param name="request">The class group creation DTO.</param>
        /// <returns>The created class group DTO.</returns>
        public ClassGroupDto CreateClassGroup(CreateClassGroupDto request)
        {
            // ── Validate input ─────────────────────────────────────────────
            AdminValidator.ValidateCreateClassGroup(request);

            // ── Verify subject exists (referential integrity) ────────────────
            var subject = _db.Subjects.Find(request.SubjectId);
            if (subject == null)
                throw new KeyNotFoundException($"Subject with ID {request.SubjectId} not found.");

            // ── Verify teacher exists (referential integrity) ───────────────
            var teacher = _db.Teachers.Find(request.TeacherId);
            if (teacher == null)
                throw new KeyNotFoundException($"Teacher with ID {request.TeacherId} not found.");

            var group = new ClassGroup
            {
                Name = request.Name.Trim(),
                GradeLevel = request.GradeLevel,
                SubjectId = request.SubjectId,
                TeacherId = request.TeacherId
            };
            _db.ClassGroups.Add(group);
            _db.SaveChanges();

            _auditLogService.LogCreate("ClassGroup", group.Id, 
                new { group.Name, group.GradeLevel, group.SubjectId, group.TeacherId }, 
                "admin@trackmygrade.com");

            return new ClassGroupDto
            {
                Id = group.Id, Name = group.Name, GradeLevel = group.GradeLevel,
                SubjectId = group.SubjectId, SubjectName = subject.Name,
                TeacherId = group.TeacherId, TeacherName = $"{teacher.FirstName} {teacher.LastName}"
            };
        }

        /// <summary>
        /// Deletes a class group. Only succeeds if no active enrollments or assignments exist.
        /// </summary>
        /// <param name="id">The ID of the class group to delete.</param>
        public void DeleteClassGroup(int id)
        {
            var classGroup = _db.ClassGroups.Find(id);
            if (classGroup == null) throw new KeyNotFoundException($"Class group with ID {id} not found.");

            // Enrollments
            var enrollmentCount = _db.StudentEnrollments.Count(e => e.ClassGroupId == id);
            if (enrollmentCount > 0)
                throw new InvalidOperationException($"Cannot delete class group: {enrollmentCount} student(s) are enrolled.");

            // Assignments
            var assignmentCount = _db.Assignments.Count(a => a.ClassGroupId == id);
            if (assignmentCount > 0)
                throw new InvalidOperationException($"Cannot delete class group: {assignmentCount} assignment(s) are associated.");

            var classSnapshot = new { classGroup.Name, classGroup.GradeLevel, classGroup.SubjectId, classGroup.TeacherId };
            _db.ClassGroups.Remove(classGroup);
            _db.SaveChanges();

            _auditLogService.LogDelete("ClassGroup", id, classSnapshot, "admin@trackmygrade.com");
        }

        /// <summary>
        /// Enrolls a student into a class group.
        /// </summary>
        /// <param name="classGroupId">The ID of the class group.</param>
        /// <param name="studentId">The ID of the student to enroll.</param>
        /// <returns>The class group DTO after enrollment.</returns>
        public ClassGroupDto EnrollStudent(int classGroupId, int studentId)
        {
            // ── Verify class group exists ──────────────────────────────────
            var classGroup = _db.ClassGroups.Find(classGroupId);
            if (classGroup == null)
                throw new KeyNotFoundException($"Class group with ID {classGroupId} not found.");

            // ── Verify student exists ──────────────────────────────────────
            var student = _db.Students.Find(studentId);
            if (student == null)
                throw new KeyNotFoundException($"Student with ID {studentId} not found.");

            // ── Check if already enrolled (prevent duplicates) ──────────────
            if (_db.StudentEnrollments.Any(e => e.ClassGroupId == classGroupId && e.StudentId == studentId))
                throw new InvalidOperationException($"Student {studentId} is already enrolled in class group {classGroupId}.");

            // ── Create enrollment ──────────────────────────────────────────
            var enrollment = new StudentEnrollment
            {
                StudentId = studentId,
                ClassGroupId = classGroupId,
                EnrolledAt = DateTime.UtcNow
            };
            _db.StudentEnrollments.Add(enrollment);
            _db.SaveChanges();

            _auditLogService.LogCreate("StudentEnrollment", enrollment.Id, 
                new { enrollment.StudentId, enrollment.ClassGroupId }, 
                "admin@trackmygrade.com");

            return GetAllClassGroups().First(cg => cg.Id == classGroupId);
        }

        /// <summary>
        /// Unenrolls a student from a class group.
        /// </summary>
        /// <param name="classGroupId">The ID of the class group.</param>
        /// <param name="studentId">The ID of the student to unenroll.</param>
        public void UnenrollStudent(int classGroupId, int studentId)
        {
            var enrollment = _db.StudentEnrollments.FirstOrDefault(
                e => e.ClassGroupId == classGroupId && e.StudentId == studentId);

            if (enrollment == null)
                throw new KeyNotFoundException(
                    $"Enrollment not found: student {studentId} in class group {classGroupId}."
                );

            var enrollmentSnapshot = new { enrollment.StudentId, enrollment.ClassGroupId };
            _db.StudentEnrollments.Remove(enrollment);
            _db.SaveChanges();

            _auditLogService.LogDelete("StudentEnrollment", enrollment.Id, enrollmentSnapshot, "admin@trackmygrade.com");
        }
    }
}
