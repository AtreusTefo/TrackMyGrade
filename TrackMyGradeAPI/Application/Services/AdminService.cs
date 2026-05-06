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
    public interface IAdminService
    {
        // ── Auth ─────────────────────────────────────────────────────────
        AdminResponseDto Login(AdminLoginDto request);

        // ── Teachers ─────────────────────────────────────────────────────
        List<AdminTeacherDto>  GetAllTeachers();
        AdminTeacherDto        CreateTeacher(AdminCreateTeacherDto request);
        void                   DeleteTeacher(int id);

        // ── Students ─────────────────────────────────────────────────────
        List<AdminStudentDto>  GetAllStudents();
        AdminStudentDto        CreateStudent(AdminCreateStudentDto request);
        AdminStudentDto        UpdateStudent(int id, AdminUpdateStudentDto request);
        void                   DeleteStudent(int id);

        // ── Courses ───────────────────────────────────────────────────────
        List<CourseDto>        GetAllCourses();
        CourseDto              CreateCourse(CreateCourseDto request);

        // ── Class Groups ─────────────────────────────────────────────────
        List<ClassGroupDto>    GetAllClassGroups();
        ClassGroupDto          CreateClassGroup(CreateClassGroupDto request);
        ClassGroupDto          EnrollStudent(int classGroupId, int studentId);
        void                   UnenrollStudent(int classGroupId, int studentId);
    }

    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper              _mapper;
        private readonly ITokenService        _tokenService;
        private readonly IAuditLogService     _auditLogService;

        public AdminService(ApplicationDbContext db, IMapper mapper, ITokenService tokenService, IAuditLogService auditLogService)
        {
            _db                = db;
            _mapper            = mapper;
            _tokenService      = tokenService;
            _auditLogService   = auditLogService;
        }

        // ── Auth ──────────────────────────────────────────────────────────

        public AdminResponseDto Login(AdminLoginDto request)
        {
            string adminEmail    = System.Configuration.ConfigurationManager.AppSettings["AdminEmail"]    ?? "admin@trackmygrade.com";
            string adminPassword = System.Configuration.ConfigurationManager.AppSettings["AdminPassword"] ?? "Admin@2026";

            if (!request.Email.Trim().Equals(adminEmail, StringComparison.OrdinalIgnoreCase) ||
                request.Password != adminPassword)
                throw new UnauthorizedAccessException("Invalid admin credentials.");

            var token = _tokenService.GenerateToken(0, "Admin", adminEmail);
            return new AdminResponseDto { Email = adminEmail, Token = token };
        }

        // ── Teachers ──────────────────────────────────────────────────────

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

        // ── Courses ───────────────────────────────────────────────────────

        public List<CourseDto> GetAllCourses()
        {
            return _db.Courses
                .Select(c => new CourseDto { Id = c.Id, Name = c.Name, Code = c.Code, Description = c.Description })
                .ToList();
        }

        public CourseDto CreateCourse(CreateCourseDto request)
        {
            // ── Validate input ─────────────────────────────────────────────
            AdminValidator.ValidateCreateCourse(request);

            // ── Check for duplicate course code (case-insensitive) ─────────
            string normalizedCode = request.Code.Trim().ToUpper();
            if (_db.Courses.Any(c => c.Code == normalizedCode))
                throw new InvalidOperationException("A course with this code already exists.");

            var course = new Course
            {
                Name = request.Name.Trim(),
                Code = normalizedCode,
                Description = request.Description?.Trim()
            };
            _db.Courses.Add(course);
            _db.SaveChanges();

            _auditLogService.LogCreate("Course", course.Id, new { course.Name, course.Code, course.Description }, "admin@trackmygrade.com");

            return new CourseDto { Id = course.Id, Name = course.Name, Code = course.Code, Description = course.Description };
        }

        // ── Class Groups ──────────────────────────────────────────────────

        public List<ClassGroupDto> GetAllClassGroups()
        {
            return _db.ClassGroups
                .Select(cg => new ClassGroupDto
                {
                    Id = cg.Id, Name = cg.Name, GradeLevel = cg.GradeLevel,
                    CourseId = cg.CourseId, CourseName = cg.Course.Name,
                    TeacherId = cg.TeacherId,
                    TeacherName = cg.Teacher.FirstName + " " + cg.Teacher.LastName
                }).ToList();
        }

        public ClassGroupDto CreateClassGroup(CreateClassGroupDto request)
        {
            // ── Validate input ─────────────────────────────────────────────
            AdminValidator.ValidateCreateClassGroup(request);

            // ── Verify course exists (referential integrity) ────────────────
            var course = _db.Courses.Find(request.CourseId);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID {request.CourseId} not found.");

            // ── Verify teacher exists (referential integrity) ───────────────
            var teacher = _db.Teachers.Find(request.TeacherId);
            if (teacher == null)
                throw new KeyNotFoundException($"Teacher with ID {request.TeacherId} not found.");

            var group = new ClassGroup
            {
                Name = request.Name.Trim(),
                GradeLevel = request.GradeLevel,
                CourseId = request.CourseId,
                TeacherId = request.TeacherId
            };
            _db.ClassGroups.Add(group);
            _db.SaveChanges();

            _auditLogService.LogCreate("ClassGroup", group.Id, 
                new { group.Name, group.GradeLevel, group.CourseId, group.TeacherId }, 
                "admin@trackmygrade.com");

            return new ClassGroupDto
            {
                Id = group.Id, Name = group.Name, GradeLevel = group.GradeLevel,
                CourseId = group.CourseId, CourseName = course.Name,
                TeacherId = group.TeacherId, TeacherName = $"{teacher.FirstName} {teacher.LastName}"
            };
        }

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
