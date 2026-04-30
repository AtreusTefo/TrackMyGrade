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
        StudentAuthResponseDto SubmitAssessments(string token, StudentSubmitAssessmentsDto request);
    }

    public class StudentAuthService : IStudentAuthService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public StudentAuthService(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public StudentAuthResponseDto Login(StudentLoginDto request)
        {
            var student = _dbContext.Students.FirstOrDefault(
                s => s.Email == request.Email && s.Password == request.Password);

            if (student == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            student.Token = Guid.NewGuid().ToString();
            _dbContext.SaveChanges();

            return _mapper.Map<StudentAuthResponseDto>(student);
        }

        public StudentAuthResponseDto GetProfile(string token)
        {
            var student = _dbContext.Students.FirstOrDefault(s => s.Token == token);
            if (student == null)
                throw new UnauthorizedAccessException("Invalid or expired token");

            return _mapper.Map<StudentAuthResponseDto>(student);
        }

        public StudentAuthResponseDto SubmitAssessments(string token, StudentSubmitAssessmentsDto request)
        {
            var student = _dbContext.Students.FirstOrDefault(s => s.Token == token);
            if (student == null)
                throw new UnauthorizedAccessException("Invalid or expired token");

            student.Assessment1 = request.Assessment1;
            student.Assessment2 = request.Assessment2;
            student.Assessment3 = request.Assessment3;
            _dbContext.SaveChanges();

            return _mapper.Map<StudentAuthResponseDto>(student);
        }
    }
}
