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
        // Teachers can READ students in their classes
        List<StudentResponseDto> GetByTeacher(int teacherId);
        StudentResponseDto       GetById(int studentId, int teacherId);
        // Create / Update / Delete moved to IAdminService
    }

    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper              _mapper;

        public StudentService(ApplicationDbContext db, IMapper mapper)
        {
            _db     = db;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all students enrolled in any ClassGroup taught by this teacher.
        /// </summary>
        public List<StudentResponseDto> GetByTeacher(int teacherId)
        {
            // Collect student IDs from all classes the teacher teaches
            var enrolledStudentIds = _db.StudentEnrollments
                .Where(e => e.ClassGroup.TeacherId == teacherId)
                .Select(e => e.StudentId)
                .Distinct()
                .ToList();

            var students = _db.Students
                .Where(s => enrolledStudentIds.Contains(s.Id))
                .ToList();

            return students.Select(s => _mapper.Map<StudentResponseDto>(s)).ToList();
        }

        /// <summary>
        /// Returns one student — only if they are enrolled in a class taught by the teacher.
        /// </summary>
        public StudentResponseDto GetById(int studentId, int teacherId)
        {
            bool hasAccess = _db.StudentEnrollments.Any(
                e => e.StudentId    == studentId &&
                     e.ClassGroup.TeacherId == teacherId);

            if (!hasAccess)
                throw new UnauthorizedAccessException(
                    "Student not found or not enrolled in your class.");

            var student = _db.Students.Find(studentId);
            if (student == null)
                throw new Exception("Student not found.");

            return _mapper.Map<StudentResponseDto>(student);
        }

        // ── Internal helper: generate unique student number ────────────────
        internal static string GenerateStudentNumber(ApplicationDbContext db)
        {
            int    year   = DateTime.Now.Year;
            string prefix = $"STU-{year}-";

            var existing = db.Students
                .Where(s => s.StudentNumber != null && s.StudentNumber.StartsWith(prefix))
                .Select(s => s.StudentNumber)
                .ToList();

            int maxSeq = 0;
            foreach (var num in existing)
            {
                if (num.Length > prefix.Length &&
                    int.TryParse(num.Substring(prefix.Length), out int seq))
                    maxSeq = Math.Max(maxSeq, seq);
            }

            return $"{prefix}{(maxSeq + 1):D4}";
        }
    }
}
