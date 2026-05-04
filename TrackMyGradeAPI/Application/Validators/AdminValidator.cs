using System;
using System.Text.RegularExpressions;
using TrackMyGradeAPI.DTOs;

namespace TrackMyGradeAPI.Validators
{
    /// <summary>Validation rules for all admin operations (teachers, students, courses, class groups).</summary>
    public static class AdminValidator
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly Regex PhoneRegex = new Regex(
            @"^\+?[0-9\-\(\)\s]{7,}$",
            RegexOptions.Compiled
        );

        // ── Teacher Validation ─────────────────────────────────────────────

        /// <summary>Validate admin create teacher request.</summary>
        public static void ValidateCreateTeacher(AdminCreateTeacherDto request)
        {
            if (request == null)
                throw new ArgumentException("Teacher data is required.");

            if (string.IsNullOrWhiteSpace(request.FirstName))
                throw new ArgumentException("First name is required.");

            if (string.IsNullOrWhiteSpace(request.LastName))
                throw new ArgumentException("Last name is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");

            if (!EmailRegex.IsMatch(request.Email))
                throw new ArgumentException("Email format is invalid.");

            if (request.FirstName.Length > 100)
                throw new ArgumentException("First name cannot exceed 100 characters.");

            if (request.LastName.Length > 100)
                throw new ArgumentException("Last name cannot exceed 100 characters.");

            if (!string.IsNullOrWhiteSpace(request.Phone) && request.Phone.Length > 20)
                throw new ArgumentException("Phone number cannot exceed 20 characters.");

            if (!string.IsNullOrWhiteSpace(request.Phone) && !PhoneRegex.IsMatch(request.Phone))
                throw new ArgumentException("Phone number format is invalid.");

            if (!string.IsNullOrWhiteSpace(request.Subject) && request.Subject.Length > 100)
                throw new ArgumentException("Subject cannot exceed 100 characters.");
        }

        // ── Student Validation ─────────────────────────────────────────────

        /// <summary>Validate admin create student request.</summary>
        public static void ValidateCreateStudent(AdminCreateStudentDto request)
        {
            if (request == null)
                throw new ArgumentException("Student data is required.");

            if (string.IsNullOrWhiteSpace(request.FirstName))
                throw new ArgumentException("First name is required.");

            if (string.IsNullOrWhiteSpace(request.LastName))
                throw new ArgumentException("Last name is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");

            if (!EmailRegex.IsMatch(request.Email))
                throw new ArgumentException("Email format is invalid.");

            if (string.IsNullOrWhiteSpace(request.OmangOrPassport))
                throw new ArgumentException("OMANG or Passport is required.");

            if (request.OmangOrPassport.Length > 20)
                throw new ArgumentException("OMANG or Passport cannot exceed 20 characters.");

            if (request.Grade < 1 || request.Grade > 12)
                throw new ArgumentException("Grade must be between 1 and 12.");

            if (request.TeacherId <= 0)
                throw new ArgumentException("Valid teacher ID is required.");

            if (request.FirstName.Length > 100)
                throw new ArgumentException("First name cannot exceed 100 characters.");

            if (request.LastName.Length > 100)
                throw new ArgumentException("Last name cannot exceed 100 characters.");

            if (!string.IsNullOrWhiteSpace(request.Phone) && request.Phone.Length > 20)
                throw new ArgumentException("Phone number cannot exceed 20 characters.");

            if (!string.IsNullOrWhiteSpace(request.Phone) && !PhoneRegex.IsMatch(request.Phone))
                throw new ArgumentException("Phone number format is invalid.");
        }

        /// <summary>Validate admin update student request.</summary>
        public static void ValidateUpdateStudent(AdminUpdateStudentDto request)
        {
            if (request == null)
                throw new ArgumentException("Student data is required.");

            ValidateCreateStudent(new AdminCreateStudentDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                OmangOrPassport = request.OmangOrPassport,
                Grade = request.Grade,
                TeacherId = request.TeacherId
            });
        }

        // ── Course Validation ──────────────────────────────────────────────

        /// <summary>Validate course creation request.</summary>
        public static void ValidateCreateCourse(CreateCourseDto request)
        {
            if (request == null)
                throw new ArgumentException("Course data is required.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Course name is required.");

            if (string.IsNullOrWhiteSpace(request.Code))
                throw new ArgumentException("Course code is required.");

            if (request.Name.Length > 200)
                throw new ArgumentException("Course name cannot exceed 200 characters.");

            if (request.Code.Length > 20)
                throw new ArgumentException("Course code cannot exceed 20 characters.");

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 500)
                throw new ArgumentException("Course description cannot exceed 500 characters.");
        }

        // ── Class Group Validation ─────────────────────────────────────────

        /// <summary>Validate class group creation request.</summary>
        public static void ValidateCreateClassGroup(CreateClassGroupDto request)
        {
            if (request == null)
                throw new ArgumentException("Class group data is required.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Class group name is required.");

            if (request.Name.Length > 100)
                throw new ArgumentException("Class group name cannot exceed 100 characters.");

            if (request.GradeLevel < 1 || request.GradeLevel > 12)
                throw new ArgumentException("Grade level must be between 1 and 12.");

            if (request.CourseId <= 0)
                throw new ArgumentException("Valid course ID is required.");

            if (request.TeacherId <= 0)
                throw new ArgumentException("Valid teacher ID is required.");
        }
    }
}
