using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public TeacherService(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public TeacherResponseDto Register(TeacherRegisterDto request)
        {
            // Check if email already exists
            if (_dbContext.Teachers.Any(t => t.Email == request.Email))
                throw new Exception("Email already registered");

            var teacher = _mapper.Map<Teacher>(request);
            teacher.Token = Guid.NewGuid().ToString();

            _dbContext.Teachers.Add(teacher);
            _dbContext.SaveChanges();

            return _mapper.Map<TeacherResponseDto>(teacher);
        }

        public TeacherResponseDto Login(TeacherLoginDto request)
        {
            var teacher = _dbContext.Teachers.FirstOrDefault(t => t.Email == request.Email &&  t.Password == request.Password);

            if (teacher == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            // Regenerate token on login
            teacher.Token = Guid.NewGuid().ToString();
            _dbContext.SaveChanges();

            return _mapper.Map<TeacherResponseDto>(teacher);
        }

        public TeacherResponseDto GetById(int id)
        {
            var teacher = _dbContext.Teachers.Find(id);
            if (teacher == null)
                throw new Exception("Teacher not found");

            return _mapper.Map<TeacherResponseDto>(teacher);
        }
    }
}
