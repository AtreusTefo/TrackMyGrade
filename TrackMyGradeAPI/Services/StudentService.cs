using System;
using System.Collections.Generic;
using System.Linq;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Services
{
    public interface IStudentService
    {
        List<StudentResponseDto> GetAllByTeacher(int teacherId);
        StudentResponseDto GetById(int id, int teacherId);
        StudentResponseDto Create(StudentCreateDto request, int teacherId);
        StudentResponseDto Update(int id, StudentUpdateDto request, int teacherId);
        void Delete(int id, int teacherId);
    }

    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _dbContext;

        public StudentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<StudentResponseDto> GetAllByTeacher(int teacherId)
        {
            var students = _dbContext.Students
                .Where(s => s.TeacherId == teacherId)
                .ToList();

            return students.Select(s => MapToResponseDto(s)).ToList();
        }

        public StudentResponseDto GetById(int id, int teacherId)
        {
            var student = _dbContext.Students.FirstOrDefault(s => s.Id == id && s.TeacherId == teacherId);
            if (student == null)
                throw new Exception("Student not found");

            return MapToResponseDto(student);
        }

        public StudentResponseDto Create(StudentCreateDto request, int teacherId)
        {
            var student = new Student
            {
                TeacherId = teacherId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Grade = request.Grade,
                Assessment1 = request.Assessment1,
                Assessment2 = request.Assessment2,
                Assessment3 = request.Assessment3
            };

            _dbContext.Students.Add(student);
            _dbContext.SaveChanges();

            return MapToResponseDto(student);
        }

        public StudentResponseDto Update(int id, StudentUpdateDto request, int teacherId)
        {
            var student = _dbContext.Students.FirstOrDefault(s => s.Id == id && s.TeacherId == teacherId);
            if (student == null)
                throw new Exception("Student not found");

            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.Email = request.Email;
            student.Phone = request.Phone;
            student.Grade = request.Grade;
            student.Assessment1 = request.Assessment1;
            student.Assessment2 = request.Assessment2;
            student.Assessment3 = request.Assessment3;

            _dbContext.SaveChanges();

            return MapToResponseDto(student);
        }

        public void Delete(int id, int teacherId)
        {
            var student = _dbContext.Students.FirstOrDefault(s => s.Id == id && s.TeacherId == teacherId);
            if (student == null)
                throw new Exception("Student not found");

            _dbContext.Students.Remove(student);
            _dbContext.SaveChanges();
        }

        private StudentResponseDto MapToResponseDto(Student student)
        {
            return new StudentResponseDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Phone = student.Phone,
                Grade = student.Grade,
                Assessment1 = student.Assessment1,
                Assessment2 = student.Assessment2,
                Assessment3 = student.Assessment3,
                Total = student.Total,
                Average = student.Average,
                Percentage = student.Percentage,
                PerformanceLevel = student.PerformanceLevel
            };
        }
    }
}
