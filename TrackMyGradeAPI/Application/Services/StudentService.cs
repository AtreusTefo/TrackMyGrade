using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Services
{
    /// <summary>
    /// Service interface for student retrieval operations.
    /// Allows teachers to view their enrolled students.
    /// </summary>
    public interface IStudentService
    {
        /// <summary>Retrieves all students enrolled in any class taught by a specific teacher.</summary>
        /// <param name="teacherId">The ID of the teacher.</param>
        /// <returns>List of student response DTOs.</returns>
        List<StudentResponseDto> GetByTeacher(int teacherId);

        /// <summary>Retrieves a specific student by ID, with authorization check for the teacher.</summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <param name="teacherId">The ID of the teacher (for authorization).</param>
        /// <returns>The student response DTO.</returns>
        StudentResponseDto       GetById(int studentId, int teacherId);
    }

    /// <summary>
    /// Implementation of IStudentService for student retrieval.
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper              _mapper;

        /// <summary>
        /// Initializes a new instance of the StudentService class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        /// <param name="mapper">The AutoMapper instance for entity-to-DTO mapping.</param>
        public StudentService(ApplicationDbContext db, IMapper mapper)
        {
            _db     = db;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all students enrolled in any ClassGroup taught by this teacher (excludes soft-deleted enrollments).
        /// </summary>
        public List<StudentResponseDto> GetByTeacher(int teacherId)
        {
            // Collect student IDs from all classes the teacher teaches (active enrollments only)
            var enrolledStudentIds = _db.StudentEnrollments
                .Where(e => e.ClassGroup.TeacherId == teacherId && !e.IsDeleted)
                .Select(e => e.StudentId)
                .Distinct()
                .ToList();

            var students = _db.Students
                .Where(s => enrolledStudentIds.Contains(s.Id))
                .ToList();

            return students.Select(s => _mapper.Map<StudentResponseDto>(s)).ToList();
        }

        /// <summary>
        /// Returns one student — only if they are enrolled in a class taught by the teacher (active enrollment only).
        /// </summary>
        public StudentResponseDto GetById(int studentId, int teacherId)
        {
            bool hasAccess = _db.StudentEnrollments.Any(
                e => e.StudentId    == studentId &&
                     e.ClassGroup.TeacherId == teacherId &&
                     !e.IsDeleted);

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
