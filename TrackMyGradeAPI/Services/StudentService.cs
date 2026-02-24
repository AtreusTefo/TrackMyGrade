using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public StudentService(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public List<StudentResponseDto> GetAllByTeacher(int teacherId)
        {
            var students = _dbContext.Students
                .Where(s => s.TeacherId == teacherId)
                .ToList();

            return students.Select(s => _mapper.Map<StudentResponseDto>(s)).ToList();
        }

        public StudentResponseDto GetById(int id, int teacherId)
        {
            var student = _dbContext.Students.FirstOrDefault(s => s.Id == id && s.TeacherId == teacherId);
            if (student == null)
                throw new Exception("Student not found");

            return _mapper.Map<StudentResponseDto>(student);
        }

        public StudentResponseDto Create(StudentCreateDto request, int teacherId)
        {
            var student = _mapper.Map<Student>(request);
            student.TeacherId = teacherId;

            _dbContext.Students.Add(student);
            _dbContext.SaveChanges();

            return _mapper.Map<StudentResponseDto>(student);
        }

        public StudentResponseDto Update(int id, StudentUpdateDto request, int teacherId)
        {
            var student = _dbContext.Students.FirstOrDefault(s => s.Id == id && s.TeacherId == teacherId);
            if (student == null)
                throw new Exception("Student not found");

            _mapper.Map(request, student);

            _dbContext.SaveChanges();

            return _mapper.Map<StudentResponseDto>(student);
        }

        public void Delete(int id, int teacherId)
        {
            var student = _dbContext.Students.FirstOrDefault(s => s.Id == id && s.TeacherId == teacherId);
            if (student == null)
                throw new Exception("Student not found");

            _dbContext.Students.Remove(student);
            _dbContext.SaveChanges();
        }
    }
}
