using System;
using System.Collections.Generic;
using System.Linq;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Services
{
    /// <summary>
    /// Service interface for assignment-related operations.
    /// Handles assignment creation, submission grading, and retrieval for both teachers and students.
    /// </summary>
    public interface IAssignmentService
    {
        /// <summary>Retrieves all assignments created by a specific teacher.</summary>
        /// <param name="teacherId">The ID of the teacher.</param>
        /// <returns>List of assignment response DTOs.</returns>
        List<AssignmentResponseDto>    GetMyAssignments(int teacherId);

        /// <summary>Creates a new assignment for a teacher.</summary>
        /// <param name="teacherId">The ID of the teacher creating the assignment.</param>
        /// <param name="request">The assignment creation DTO.</param>
        /// <returns>The created assignment response DTO.</returns>
        AssignmentResponseDto          CreateAssignment(int teacherId, AssignmentCreateDto request);

        /// <summary>Retrieves all submissions for a specific assignment.</summary>
        /// <param name="assignmentId">The ID of the assignment.</param>
        /// <param name="teacherId">The ID of the teacher (for authorization).</param>
        /// <returns>List of submission response DTOs.</returns>
        List<SubmissionResponseDto>    GetSubmissions(int assignmentId, int teacherId);

        /// <summary>Grades a student submission.</summary>
        /// <param name="submissionId">The ID of the submission to grade.</param>
        /// <param name="teacherId">The ID of the teacher grading (for authorization).</param>
        /// <param name="request">The grading DTO containing score and feedback.</param>
        /// <returns>The updated submission response DTO.</returns>
        SubmissionResponseDto          GradeSubmission(int submissionId, int teacherId, GradingDto request);

        /// <summary>Retrieves all assignments assigned to a specific student.</summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>List of assignment response DTOs.</returns>
        List<AssignmentResponseDto>    GetStudentAssignments(int studentId);

        /// <summary>Submits an assignment for a student.</summary>
        /// <param name="assignmentId">The ID of the assignment being submitted.</param>
        /// <param name="studentId">The ID of the student submitting.</param>
        /// <param name="request">The submission creation DTO.</param>
        /// <returns>The created submission response DTO.</returns>
        SubmissionResponseDto          SubmitAssignment(int assignmentId, int studentId, SubmissionCreateDto request);

        /// <summary>Retrieves all submissions made by a specific student.</summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>List of submission response DTOs.</returns>
        List<SubmissionResponseDto>    GetMySubmissions(int studentId);
    }

    /// <summary>
    /// Implementation of IAssignmentService for managing assignments and submissions.
    /// </summary>
    public class AssignmentService : IAssignmentService
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the AssignmentService class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public AssignmentService(ApplicationDbContext db) { _db = db; }

        // ── Teacher: view my assignments ───────────────────────────────────

        /// <summary>
        /// Retrieves all assignments created by a specific teacher.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher.</param>
        /// <returns>List of assignment response DTOs.</returns>
        public List<AssignmentResponseDto> GetMyAssignments(int teacherId)
        {
            return _db.Assignments
                .Where(a => a.CreatedByTeacherId == teacherId)
                .Select(a => new AssignmentResponseDto
                {
                    Id               = a.Id,
                    Title            = a.Title,
                    Description      = a.Description,
                    DueDate          = a.DueDate,
                    MaxScore         = a.MaxScore,
                    ClassGroupId     = a.ClassGroupId,
                    ClassGroupName   = a.ClassGroup.Name,
                    CreatedAt        = a.CreatedAt,
                    SubmissionCount  = a.Submissions.Count()
                })
                .ToList();
        }

        // ── Teacher: create assignment ─────────────────────────────────────

        /// <summary>
        /// Creates a new assignment for a teacher's class group.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher creating the assignment.</param>
        /// <param name="request">The assignment creation DTO.</param>
        /// <returns>The created assignment response DTO.</returns>
        public AssignmentResponseDto CreateAssignment(int teacherId, AssignmentCreateDto request)
        {
            var classGroup = _db.ClassGroups.Find(request.ClassGroupId);
            if (classGroup == null)
                throw new Exception("Class group not found.");

            if (classGroup.TeacherId != teacherId)
                throw new UnauthorizedAccessException(
                    "You can only create assignments for your own classes.");

            if (request.MaxScore <= 0)
                throw new ArgumentException("MaxScore must be greater than zero.");

            if (request.DueDate <= DateTime.UtcNow)
                throw new ArgumentException("Due date must be in the future.");

            var assignment = new Assignment
            {
                Title              = request.Title.Trim(),
                Description        = request.Description?.Trim(),
                DueDate            = request.DueDate,
                MaxScore           = request.MaxScore,
                ClassGroupId       = request.ClassGroupId,
                CreatedByTeacherId = teacherId,
                CreatedAt          = DateTime.UtcNow
            };

            _db.Assignments.Add(assignment);
            _db.SaveChanges();

            return new AssignmentResponseDto
            {
                Id = assignment.Id, Title = assignment.Title,
                Description = assignment.Description, DueDate = assignment.DueDate,
                MaxScore = assignment.MaxScore, ClassGroupId = assignment.ClassGroupId,
                ClassGroupName = classGroup.Name, CreatedAt = assignment.CreatedAt,
                SubmissionCount = 0
            };
        }

        // ── Teacher: view submissions for an assignment ────────────────────

        /// <summary>
        /// Retrieves all submissions for a specific assignment.
        /// </summary>
        /// <param name="assignmentId">The ID of the assignment.</param>
        /// <param name="teacherId">The ID of the teacher (for authorization).</param>
        /// <returns>List of submission response DTOs.</returns>
        public List<SubmissionResponseDto> GetSubmissions(int assignmentId, int teacherId)
        {
            var assignment = _db.Assignments.Find(assignmentId);
            if (assignment == null) throw new Exception("Assignment not found.");

            if (assignment.CreatedByTeacherId != teacherId)
                throw new UnauthorizedAccessException("Access denied.");

            return _db.AssignmentSubmissions
                .Where(s => s.AssignmentId == assignmentId)
                .Select(s => new SubmissionResponseDto
                {
                    Id            = s.Id,
                    AssignmentId  = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    StudentId     = s.StudentId,
                    StudentName   = s.Student.FirstName + " " + s.Student.LastName,
                    SubmittedAt   = s.SubmittedAt,
                    Content       = s.Content,
                    Score         = s.Score,
                    MaxScore      = s.Assignment.MaxScore,
                    Feedback      = s.Feedback,
                    Status        = s.Status
                })
                .ToList();
        }

        // ── Teacher: grade a submission ────────────────────────────────────

        /// <summary>
        /// Grades a student submission with a score and optional feedback.
        /// </summary>
        /// <param name="submissionId">The ID of the submission to grade.</param>
        /// <param name="teacherId">The ID of the teacher grading (for authorization).</param>
        /// <param name="request">The grading DTO containing score and feedback.</param>
        /// <returns>The updated submission response DTO.</returns>
        public SubmissionResponseDto GradeSubmission(int submissionId, int teacherId, GradingDto request)
        {
            var submission = _db.AssignmentSubmissions.Find(submissionId);
            if (submission == null) throw new Exception("Submission not found.");

            // Verify the assignment belongs to this teacher
            var assignment = _db.Assignments.Find(submission.AssignmentId);
            if (assignment == null || assignment.CreatedByTeacherId != teacherId)
                throw new UnauthorizedAccessException("Access denied.");

            if (request.Score < 0 || request.Score > assignment.MaxScore)
                throw new ArgumentException($"Score must be between 0 and {assignment.MaxScore}.");

            submission.Score    = request.Score;
            submission.Feedback = request.Feedback?.Trim();
            submission.Status   = SubmissionStatus.Graded;
            _db.SaveChanges();

            return MapSubmission(submission, assignment);
        }

        // ── Student: view assignments in enrolled classes ──────────────────

        /// <summary>
        /// Retrieves all assignments assigned to a specific student across enrolled classes.
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>List of assignment response DTOs.</returns>
        public List<AssignmentResponseDto> GetStudentAssignments(int studentId)
        {
            // Get all class IDs the student is enrolled in
            var classIds = _db.StudentEnrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.ClassGroupId)
                .ToList();

            return _db.Assignments
                .Where(a => classIds.Contains(a.ClassGroupId))
                .Select(a => new AssignmentResponseDto
                {
                    Id             = a.Id,
                    Title          = a.Title,
                    Description    = a.Description,
                    DueDate        = a.DueDate,
                    MaxScore       = a.MaxScore,
                    ClassGroupId   = a.ClassGroupId,
                    ClassGroupName = a.ClassGroup.Name,
                    CreatedAt      = a.CreatedAt,
                    // student's own submission status
                    StudentSubmissionStatus = a.Submissions
                        .Where(s => s.StudentId == studentId)
                        .Select(s => s.Status)
                        .FirstOrDefault() ?? "Not Submitted"
                })
                .ToList();
        }

        // ── Student: submit an assignment ──────────────────────────────────

        /// <summary>
        /// Submits an assignment for a student.
        /// </summary>
        /// <param name="assignmentId">The ID of the assignment being submitted.</param>
        /// <param name="studentId">The ID of the student submitting.</param>
        /// <param name="request">The submission creation DTO.</param>
        /// <returns>The created submission response DTO.</returns>
        public SubmissionResponseDto SubmitAssignment(int assignmentId, int studentId, SubmissionCreateDto request)
        {
            var assignment = _db.Assignments.Find(assignmentId);
            if (assignment == null) throw new Exception("Assignment not found.");

            // Confirm student is enrolled in the class
            bool enrolled = _db.StudentEnrollments.Any(
                e => e.StudentId == studentId && e.ClassGroupId == assignment.ClassGroupId);

            if (!enrolled)
                throw new UnauthorizedAccessException(
                    "You are not enrolled in the class this assignment belongs to.");

            // Prevent duplicate submissions
            if (_db.AssignmentSubmissions.Any(
                    s => s.AssignmentId == assignmentId && s.StudentId == studentId))
                throw new InvalidOperationException(
                    "You have already submitted this assignment.");

            string status = DateTime.UtcNow > assignment.DueDate
                ? SubmissionStatus.Late
                : SubmissionStatus.Pending;

            var submission = new AssignmentSubmission
            {
                AssignmentId = assignmentId,
                StudentId    = studentId,
                SubmittedAt  = DateTime.UtcNow,
                Content      = request.Content?.Trim(),
                Status       = status
            };

            _db.AssignmentSubmissions.Add(submission);
            _db.SaveChanges();

            return MapSubmission(submission, assignment);
        }

        // ── Student: view own submissions ──────────────────────────────────

        /// <summary>
        /// Retrieves all submissions made by a specific student.
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>List of submission response DTOs.</returns>
        public List<SubmissionResponseDto> GetMySubmissions(int studentId)
        {
            return _db.AssignmentSubmissions
                .Where(s => s.StudentId == studentId)
                .Select(s => new SubmissionResponseDto
                {
                    Id              = s.Id,
                    AssignmentId    = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    StudentId       = s.StudentId,
                    StudentName     = s.Student.FirstName + " " + s.Student.LastName,
                    SubmittedAt     = s.SubmittedAt,
                    Content         = s.Content,
                    Score           = s.Score,
                    MaxScore        = s.Assignment.MaxScore,
                    Feedback        = s.Feedback,
                    Status          = s.Status
                })
                .ToList();
        }

        // ── Helper ─────────────────────────────────────────────────────────

        private SubmissionResponseDto MapSubmission(AssignmentSubmission s, Assignment a)
        {
            var student = _db.Students.Find(s.StudentId);
            return new SubmissionResponseDto
            {
                Id              = s.Id,
                AssignmentId    = s.AssignmentId,
                AssignmentTitle = a.Title,
                StudentId       = s.StudentId,
                StudentName     = student != null ? $"{student.FirstName} {student.LastName}" : "",
                SubmittedAt     = s.SubmittedAt,
                Content         = s.Content,
                Score           = s.Score,
                MaxScore        = a.MaxScore,
                Feedback        = s.Feedback,
                Status          = s.Status
            };
        }
    }
}
