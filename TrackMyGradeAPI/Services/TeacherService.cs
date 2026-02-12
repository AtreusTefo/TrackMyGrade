using System;
using System.Collections.Generic;
using System.Linq;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Services
{
    public interface ITeacherService
    {
        TeacherResponseDto Register(TeacherRegisterDto request);
        TeacherResponseDto Login(TeacherLoginDto request);
        TeacherResponseDto GetById(int id);
    }

    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _dbContext;

        public TeacherService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public TeacherResponseDto Register(TeacherRegisterDto request)
        {
            // Check if email already exists
            if (_dbContext.Teachers.Any(t => t.Email == request.Email))
                throw new Exception("Email already registered");

            var teacher = new Teacher
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Subject = request.Subject,
                Password = request.Password,
                Token = Guid.NewGuid().ToString()
            };

            _dbContext.Teachers.Add(teacher);
            _dbContext.SaveChanges();

            return new TeacherResponseDto
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email,
                Phone = teacher.Phone,
                Subject = teacher.Subject,
                Token = teacher.Token
            };
        }

        public TeacherResponseDto Login(TeacherLoginDto request)
        {
            var teacher = _dbContext.Teachers.FirstOrDefault(t => t.Email == request.Email &&  t.Password == request.Password);

            if (teacher == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            // Regenerate token on login
            teacher.Token = Guid.NewGuid().ToString();
            _dbContext.SaveChanges();

            return new TeacherResponseDto
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email,
                Phone = teacher.Phone,
                Subject = teacher.Subject,
                Token = teacher.Token
            };
        }

        public TeacherResponseDto GetById(int id)
        {
            var teacher = _dbContext.Teachers.Find(id);
            if (teacher == null)
                throw new Exception("Teacher not found");

            return new TeacherResponseDto
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email,
                Phone = teacher.Phone,
                Subject = teacher.Subject,
                Token = teacher.Token
            };
        }
    }
}
