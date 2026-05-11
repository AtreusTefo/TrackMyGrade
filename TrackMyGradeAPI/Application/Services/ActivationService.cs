using System;
using System.Linq;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Services
{
    public interface IActivationService
    {
        ActivationResponseDto Activate(ActivateAccountDto request);
        ActivationStatusDto   CheckStatus(string token, string role);
    }

    public class ActivationService : IActivationService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService        _tokenService;

        public ActivationService(ApplicationDbContext db, ITokenService tokenService)
        {
            _db           = db;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Activates a new teacher or student account.
        /// Validates the activation token, hashes the new password, and issues a JWT.
        /// </summary>
        public ActivationResponseDto Activate(ActivateAccountDto request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new ArgumentException("New password is required.");

            if (request.NewPassword != request.ConfirmPassword)
                throw new ArgumentException("Passwords do not match.");

            if (request.NewPassword.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.");

            string hash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            if (request.Role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                return ActivateTeacher(request.ActivationToken, hash);

            if (request.Role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                return ActivateStudent(request.ActivationToken, hash);

            throw new ArgumentException("Role must be 'Teacher' or 'Student'.");
        }

        /// <summary>
        /// Returns the activation status for a given token + role without activating.
        /// Used by the Angular activation page to pre-populate the user's name.
        /// </summary>
        public ActivationStatusDto CheckStatus(string token, string role)
        {
            if (role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
            {
                var teacher = _db.Teachers.FirstOrDefault(t => t.ActivationToken == token);
                if (teacher == null)
                    throw new Exception("Invalid or already-used activation token.");

                return new ActivationStatusDto
                {
                    IsActivated = teacher.IsActivated,
                    FullName    = $"{teacher.FirstName} {teacher.LastName}",
                    Email       = teacher.Email,
                    Role        = "Teacher"
                };
            }

            if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                var student = _db.Students.FirstOrDefault(s => s.ActivationToken == token);
                if (student == null)
                    throw new Exception("Invalid or already-used activation token.");

                return new ActivationStatusDto
                {
                    IsActivated = student.IsActivated,
                    FullName    = $"{student.FirstName} {student.LastName}",
                    Email       = student.Email,
                    Role        = "Student"
                };
            }

            throw new ArgumentException("Role must be 'Teacher' or 'Student'.");
        }

        // ── Private helpers ────────────────────────────────────────────────

        private ActivationResponseDto ActivateTeacher(string activationToken, string passwordHash)
        {
            var teacher = _db.Teachers.FirstOrDefault(t => t.ActivationToken == activationToken);

            if (teacher == null)
                throw new Exception("Invalid or already-used activation token.");

            if (teacher.IsActivated)
                throw new InvalidOperationException("This account has already been activated.");

            teacher.PasswordHash    = passwordHash;
            teacher.IsActivated     = true;
            teacher.ActivatedAt     = DateTime.UtcNow;
            teacher.ActivationToken = null;   // invalidate — one-time use only
            teacher.Token           = _tokenService.GenerateToken(teacher.Id, "Teacher", teacher.Email);

            _db.SaveChanges();

            return new ActivationResponseDto
            {
                UserId    = teacher.Id,
                FullName  = $"{teacher.FirstName} {teacher.LastName}",
                Email     = teacher.Email,
                Role      = "Teacher",
                Token     = teacher.Token,
                Dashboard = "/teacher-dashboard"
            };
        }

        private ActivationResponseDto ActivateStudent(string activationToken, string passwordHash)
        {
            var student = _db.Students.FirstOrDefault(s => s.ActivationToken == activationToken);

            if (student == null)
                throw new Exception("Invalid or already-used activation token.");

            if (student.IsActivated)
                throw new InvalidOperationException("This account has already been activated.");

            student.PasswordHash    = passwordHash;
            student.IsActivated     = true;
            student.ActivatedAt     = DateTime.UtcNow;
            student.ActivationToken = null;
            student.Token           = _tokenService.GenerateToken(student.Id, "Student", student.Email);

            _db.SaveChanges();

            return new ActivationResponseDto
            {
                UserId    = student.Id,
                FullName  = $"{student.FirstName} {student.LastName}",
                Email     = student.Email,
                Role      = "Student",
                Token     = student.Token,
                Dashboard = "/student-dashboard"
            };
        }

        // Add a default admin
        public void AddDefaultAdmin()
        {
            if (! _db.Admins.Any(a => a.Email == "admin@trackmygrade.com"))
            {
                _db.Admins.Add(new Admin
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@trackmygrade.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@123")  // Hash the password
                });
                _db.SaveChanges();
            }
        }
    }
}
