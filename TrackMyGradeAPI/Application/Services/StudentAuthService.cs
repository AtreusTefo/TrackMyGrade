using System;
using System.Linq;
using AutoMapper;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Services
{
    public interface IStudentAuthService
    {
        StudentAuthResponseDto Login(StudentLoginDto request);
        StudentAuthResponseDto GetProfile(string token);
        // NOTE: SubmitAssessments removed — students submit assignments, teachers grade them.
    }

    public class StudentAuthService : IStudentAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper              _mapper;
        private readonly ITokenService        _tokenService;

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
