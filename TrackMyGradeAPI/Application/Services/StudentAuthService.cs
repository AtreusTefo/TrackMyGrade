using System;
using System.Linq;
using AutoMapper;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Infrastructure;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Services
{
    /// <summary>
    /// Service interface for student authentication operations.
    /// Handles login and profile retrieval for students.
    /// </summary>
    public interface IStudentAuthService
    {
        /// <summary>Authenticates a student with email and password, returning a JWT token.</summary>
        /// <param name="request">The student login credentials.</param>
        /// <returns>The student authentication response with JWT token.</returns>
        StudentAuthResponseDto Login(StudentLoginDto request);

        /// <summary>Retrieves the profile of an authenticated student using a JWT token.</summary>
        /// <param name="token">The JWT token of the student.</param>
        /// <returns>The student authentication response with profile information.</returns>
        StudentAuthResponseDto GetProfile(string token);
    }

    /// <summary>
    /// Implementation of IStudentAuthService for student authentication.
    /// </summary>
    public class StudentAuthService : IStudentAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper              _mapper;
        private readonly ITokenService        _tokenService;

        /// <summary>
        /// Initializes a new instance of the StudentAuthService class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        /// <param name="mapper">The AutoMapper instance for entity-to-DTO mapping.</param>
        /// <param name="tokenService">The token service for JWT operations.</param>
        public StudentAuthService(ApplicationDbContext db, IMapper mapper, ITokenService tokenService)
        {
            _db           = db;
            _mapper       = mapper;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Authenticates a student with email + password.
        /// Returns a signed JWT on success.
        /// Students must have activated their account before logging in.
        /// </summary>
        public StudentAuthResponseDto Login(StudentLoginDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email)
                                || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Email and password are required.");

            var student = _db.Students.FirstOrDefault(
                s => s.Email == request.Email.Trim().ToLower());

            if (student == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!student.IsActivated)
                throw new InvalidOperationException(
                    "Account not yet activated. Please use your activation link to set a password.");

            // Issue JWT
            student.Token = _tokenService.GenerateToken(student.Id, "Student", student.Email);
            _db.SaveChanges();

            return _mapper.Map<StudentAuthResponseDto>(student);
        }

        /// <summary>
        /// Returns the authenticated student's profile.
        /// Accepts the JWT string directly (extracted from Bearer header by the controller).
        /// </summary>
        public StudentAuthResponseDto GetProfile(string token)
        {
            var student = _db.Students.FirstOrDefault(s => s.Token == token);
            if (student == null)
                throw new UnauthorizedAccessException("Invalid or expired token.");

            return _mapper.Map<StudentAuthResponseDto>(student);
        }
    }
}
