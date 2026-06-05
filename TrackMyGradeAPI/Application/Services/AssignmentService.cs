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
        /// Retrieves all assignments created by a specific teacher (excludes soft-deleted records).
        /// </summary>
        /// <param name="teacherId">The ID of the teacher.</param>
        /// <returns>List of assignment response DTOs.</returns>
        public List<AssignmentResponseDto> GetMyAssignments(int teacherId)
        {
            return _db.Assignments
                .Where(a => a.CreatedByTeacherId == teacherId && !a.IsDeleted)
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
                    SubmissionCount  = a.Submissions.Where(s => !s.IsDeleted).Count()
                })
                .ToList();
        }

        // ── Teacher: create assignment ─────────────────────────────────────

        /// <summary>
        /// Creates a new assignment for a teacher's class group.
        /// Validates FK references (ClassGroupId, CreatedByTeacherId) exist before creating.
        /// Uses transaction to ensure atomicity: assignment creation must succeed or roll back entirely.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher creating the assignment.</param>
        /// <param name="request">The assignment creation DTO.</param>
        /// <returns>The created assignment response DTO.</returns>
        public AssignmentResponseDto CreateAssignment(int teacherId, AssignmentCreateDto request)
        {
            // ── Validate input ─────────────────────────────────────────────
            if (request.MaxScore <= 0)
                throw new ArgumentException("MaxScore must be greater than zero.");

            if (request.DueDate <= DateTime.UtcNow)
                throw new ArgumentException("Due date must be in the future.");

            // ── FK: Verify ClassGroupId exists and belongs to this teacher ──
            var classGroup = _db.ClassGroups.Find(request.ClassGroupId);
            if (classGroup == null)
                throw new KeyNotFoundException($"Class group with ID {request.ClassGroupId} not found.");

            if (classGroup.TeacherId != teacherId)
                throw new UnauthorizedAccessException(
                    "You can only create assignments for your own classes.");

            // ── FK: Verify teacher exists (redundant but explicit check) ───
            if (!_db.Teachers.Any(t => t.Id == teacherId))
                throw new KeyNotFoundException($"Teacher with ID {teacherId} not found.");

            // ── Transaction: Wrap assignment creation for atomicity ────────
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var assignment = new Assignment
                    {
                        Title              = request.Title.Trim(),
                        Description        = request.Description?.Trim(),
                        DueDate            = request.DueDate,
                        MaxScore           = request.MaxScore,
                        ClassGroupId       = request.ClassGroupId,
                        CreatedByTeacherId = teacherId,
                        CreatedAt          = DateTime.UtcNow,
                        UpdatedAt          = DateTime.UtcNow
                    };

                    _db.Assignments.Add(assignment);
                    _db.SaveChanges();

                    transaction.Commit();

                    return new AssignmentResponseDto
                    {
                        Id = assignment.Id, Title = assignment.Title,
                        Description = assignment.Description, DueDate = assignment.DueDate,
                        MaxScore = assignment.MaxScore, ClassGroupId = assignment.ClassGroupId,
                        ClassGroupName = classGroup.Name, CreatedAt = assignment.CreatedAt,
                        SubmissionCount = 0
                    };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Failed to create assignment. Transaction rolled back.", ex);
                }
            }
        }

        // ── Teacher: view submissions for an assignment ────────────────────

        /// <summary>
        /// Retrieves all submissions for a specific assignment (excludes soft-deleted).
        /// </summary>
        /// <param name="assignmentId">The ID of the assignment.</param>
        /// <param name="teacherId">The ID of the teacher (for authorization).</param>
        /// <returns>List of submission response DTOs.</returns>
        public List<SubmissionResponseDto> GetSubmissions(int assignmentId, int teacherId)
        {
            var assignment = _db.Assignments.Find(assignmentId);
            if (assignment == null) throw new KeyNotFoundException($"Assignment with ID {assignmentId} not found.");

            if (assignment.CreatedByTeacherId != teacherId)
                throw new UnauthorizedAccessException("You did not create this assignment; access denied.");

            return _db.AssignmentSubmissions
                .Where(s => s.AssignmentId == assignmentId && !s.IsDeleted)
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
        /// Validates FK references (SubmissionId, AssignmentId, TeacherId authorization) exist and are valid.
        /// Enforces status state machine: only Pending or Late submissions can be graded; once Graded, no re-grading without explicit state reset.
        /// Uses transaction to ensure atomicity: grade update must succeed or roll back entirely.
        /// </summary>
        /// <param name="submissionId">The ID of the submission to grade.</param>
        /// <param name="teacherId">The ID of the teacher grading (for authorization).</param>
        /// <param name="request">The grading DTO containing score and feedback.</param>
        /// <returns>The updated submission response DTO.</returns>
        public SubmissionResponseDto GradeSubmission(int submissionId, int teacherId, GradingDto request)
        {
            // ── FK: Verify SubmissionId exists ─────────────────────────────
            var submission = _db.AssignmentSubmissions.Find(submissionId);
            if (submission == null)
                throw new KeyNotFoundException($"Submission with ID {submissionId} not found.");

            // ── FK: Verify AssignmentId exists (via submission) ────────────
            var assignment = _db.Assignments.Find(submission.AssignmentId);
            if (assignment == null)
                throw new KeyNotFoundException($"Assignment with ID {submission.AssignmentId} not found.");

            // ── Authorization: Verify teacher created this assignment ───────
            if (assignment.CreatedByTeacherId != teacherId)
                throw new UnauthorizedAccessException(
                    $"Teacher {teacherId} did not create this assignment; access denied.");

            // ── State Machine: Validate submission status transition ────────
            // Valid transitions: (Pending or Late) → Graded
            // Invalid transitions: Graded → Graded (no re-grading) or any other invalid state
            if (submission.Status == SubmissionStatus.Graded)
                throw new InvalidOperationException(
                    "This submission has already been graded. Re-grading is not permitted.");

            if (submission.Status != SubmissionStatus.Pending && submission.Status != SubmissionStatus.Late)
                throw new InvalidOperationException(
                    $"Invalid submission status '{submission.Status}' for grading. Only 'Pending' or 'Late' submissions can be graded.");

            // ── Business Logic: Validate score range ──────────────────────
            if (request.Score < 0 || request.Score > assignment.MaxScore)
                throw new ArgumentException($"Score must be between 0 and {assignment.MaxScore}.");

            // ── Transaction: Wrap grade update for atomicity ────────────────
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    submission.Score    = request.Score;
                    submission.Feedback = request.Feedback?.Trim();
                    submission.Status   = SubmissionStatus.Graded;  // State transition: → Graded
                    submission.UpdatedAt = DateTime.UtcNow;
                    _db.SaveChanges();

                    transaction.Commit();

                    return MapSubmission(submission, assignment);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Failed to grade submission. Transaction rolled back.", ex);
                }
            }
        }

        // ── Student: view assignments in enrolled classes ──────────────────

        /// <summary>
        /// Retrieves all assignments assigned to a specific student across enrolled classes (excludes soft-deleted).
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>List of assignment response DTOs.</returns>
        public List<AssignmentResponseDto> GetStudentAssignments(int studentId)
        {
            // Get all class IDs the student is enrolled in (active enrollments only)
            var classIds = _db.StudentEnrollments
                .Where(e => e.StudentId == studentId && !e.IsDeleted)
                .Select(e => e.ClassGroupId)
                .ToList();

            return _db.Assignments
                .Where(a => classIds.Contains(a.ClassGroupId) && !a.IsDeleted)
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
                        .Where(s => s.StudentId == studentId && !s.IsDeleted)
                        .Select(s => s.Status)
                        .FirstOrDefault() ?? "Not Submitted"
                })
                .ToList();
        }

        // ── Student: submit an assignment ──────────────────────────────────

        /// <summary>
        /// Submits an assignment for a student.
        /// Validates FK references (AssignmentId, StudentId, ClassGroupId for enrollment) exist before creating submission.
        /// Uses transaction to ensure atomicity: submission creation must succeed or roll back entirely.
        /// </summary>
        /// <param name="assignmentId">The ID of the assignment being submitted.</param>
        /// <param name="studentId">The ID of the student submitting.</param>
        /// <param name="request">The submission creation DTO.</param>
        /// <returns>The created submission response DTO.</returns>
        public SubmissionResponseDto SubmitAssignment(int assignmentId, int studentId, SubmissionCreateDto request)
        {
            // ── FK: Verify AssignmentId exists ─────────────────────────────
            var assignment = _db.Assignments.Find(assignmentId);
            if (assignment == null)
                throw new KeyNotFoundException($"Assignment with ID {assignmentId} not found.");

            // ── FK: Verify StudentId exists ────────────────────────────────
            if (!_db.Students.Any(s => s.Id == studentId))
                throw new KeyNotFoundException($"Student with ID {studentId} not found.");

            // ── Business Logic: Confirm student is enrolled in the class ────
            bool enrolled = _db.StudentEnrollments.Any(
                e => e.StudentId == studentId && e.ClassGroupId == assignment.ClassGroupId && !e.IsDeleted);

            if (!enrolled)
                throw new UnauthorizedAccessException(
                    "You are not enrolled in the class this assignment belongs to.");

            // ── Business Logic: Prevent duplicate submissions ────────────────
            if (_db.AssignmentSubmissions.Any(
                    s => s.AssignmentId == assignmentId && s.StudentId == studentId && !s.IsDeleted))
                throw new InvalidOperationException(
                    "You have already submitted this assignment.");

            string status = DateTime.UtcNow > assignment.DueDate
                ? SubmissionStatus.Late
                : SubmissionStatus.Pending;

            // ── Transaction: Wrap submission creation for atomicity ────────
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var submission = new AssignmentSubmission
                    {
                        AssignmentId = assignmentId,
                        StudentId    = studentId,
                        SubmittedAt  = DateTime.UtcNow,
                        UpdatedAt    = DateTime.UtcNow,
                        Content      = request.Content?.Trim(),
                        Status       = status
                    };

                    _db.AssignmentSubmissions.Add(submission);
                    _db.SaveChanges();

                    transaction.Commit();

                    return MapSubmission(submission, assignment);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Failed to submit assignment. Transaction rolled back.", ex);
                }
            }
        }

        // ── Student: view own submissions ──────────────────────────────────

        /// <summary>
        /// Retrieves all submissions made by a specific student (excludes soft-deleted).
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>List of submission response DTOs.</returns>
        public List<SubmissionResponseDto> GetMySubmissions(int studentId)
        {
            return _db.AssignmentSubmissions
                .Where(s => s.StudentId == studentId && !s.IsDeleted)
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
