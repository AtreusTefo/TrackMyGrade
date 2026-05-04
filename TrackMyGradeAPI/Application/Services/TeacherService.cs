using System;
using System.Linq;
using AutoMapper;
using BCrypt.Net;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Services
{
    public interface ITeacherService
    {
        TeacherResponseDto Login(TeacherLoginDto request);
        TeacherResponseDto GetById(int id);
    }

    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper              _mapper;
        private readonly ITokenService        _tokenService;

        public TeacherService(ApplicationDbContext db, IMapper mapper, ITokenService tokenService)
        {
            _db           = db;
            _mapper       = mapper;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Authenticates a teacher with email + password.
        /// Returns a signed JWT on success.
        /// Teachers are created by Admins; they cannot self-register.
        /// </summary>
        public TeacherResponseDto Login(TeacherLoginDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email)
                                || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Email and password are required.");

            var teacher = _db.Teachers.FirstOrDefault(
                t => t.Email == request.Email.Trim().ToLower());

            if (teacher == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            // Verify BCrypt hash
            if (!BCrypt.Net.BCrypt.Verify(request.Password, teacher.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!teacher.IsActivated)
                throw new InvalidOperationException(
                    "Account not yet activated. Please check your activation email.");

            // Issue JWT
            teacher.Token = _tokenService.GenerateToken(teacher.Id, "Teacher", teacher.Email);
            _db.SaveChanges();

            return _mapper.Map<TeacherResponseDto>(teacher);
        }

        public TeacherResponseDto GetById(int id)
        {
            var teacher = _db.Teachers.Find(id);
            if (teacher == null)
                throw new Exception("Teacher not found.");

            return _mapper.Map<TeacherResponseDto>(teacher);
        }
    }
}
