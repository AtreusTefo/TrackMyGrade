using System;
using System.Collections.Generic;

namespace TrackMyGradeAPI.Models
{
    // ── Existing models (Admin, Teacher & Student) updated below ─────────────

    /// <summary>Teacher account used for student assignments, class management, and grading.</summary>
    public class Teacher
    {
        /// <summary>Primary key for the teacher record.</summary>
        public int Id { get; set; }

        /// <summary>Teacher given name.</summary>
        public string FirstName { get; set; }

        /// <summary>Teacher family name.</summary>
        public string LastName { get; set; }

        /// <summary>Teacher email address used for login and notifications.</summary>
        public string Email { get; set; }

        /// <summary>Teacher contact phone number.</summary>
        public string Phone { get; set; }

        /// <summary>Subject the teacher is assigned to teach.</summary>
        public string Subject { get; set; }

        /// <summary>BCrypt hash of the teacher password.</summary>
        public string PasswordHash { get; set; }

        /// <summary>Session token used for teacher authentication.</summary>
        public string Token { get; set; }

        // Account activation
        /// <summary>Whether the teacher account is activated.</summary>
        public bool IsActivated { get; set; } = false;

        /// <summary>Token issued for account activation flows.</summary>
        public string ActivationToken { get; set; }

        /// <summary>UTC timestamp when the teacher account was activated.</summary>
        public DateTime? ActivatedAt { get; set; }

        // Navigation
        /// <summary>Class groups this teacher is responsible for.</summary>
        public virtual ICollection<ClassGroup> ClassGroups { get; set; }

        /// <summary>Students assigned to this teacher.</summary>
        public virtual ICollection<Student> Students { get; set; }

        /// <summary>Assignments created by this teacher.</summary>
        public virtual ICollection<Assignment> Assignments { get; set; }
    }

    /// <summary>Student account used within the grading and enrollment system.</summary>
    public class Student
    {
        /// <summary>Primary key for the student record.</summary>
        public int Id { get; set; }

        /// <summary>Unique student identifier used by the school.</summary>
        public string StudentNumber { get; set; }

        /// <summary>Primary teacher identifier for legacy reads.</summary>
        public int TeacherId { get; set; }

        /// <summary>Student given name.</summary>
        public string FirstName { get; set; }

        /// <summary>Student family name.</summary>
        public string LastName { get; set; }

        /// <summary>Student email address used for login and notifications.</summary>
        public string Email { get; set; }

        /// <summary>Student contact phone number.</summary>
        public string Phone { get; set; }

        /// <summary>Omang or passport number for student identity verification.</summary>
        public string OmangOrPassport { get; set; }

        /// <summary>Grade level for the student.</summary>
        public int Grade { get; set; }

        /// <summary>BCrypt hash of the student password.</summary>
        public string PasswordHash { get; set; }

        /// <summary>Session token used for student authentication.</summary>
        public string Token { get; set; }

        // Account activation
        /// <summary>Whether the student account is activated.</summary>
        public bool IsActivated { get; set; } = false;

        /// <summary>Token issued for account activation flows.</summary>
        public string ActivationToken { get; set; }

        /// <summary>UTC timestamp when the student account was activated.</summary>
        public DateTime? ActivatedAt { get; set; }

        // Navigation
        /// <summary>Primary teacher assigned to the student.</summary>
        public virtual Teacher Teacher { get; set; }

        /// <summary>Enrollments for the student.</summary>
        public virtual ICollection<StudentEnrollment> Enrollments { get; set; }

        /// <summary>Assignment submissions made by the student.</summary>
        public virtual ICollection<AssignmentSubmission> Submissions { get; set; }
    }

    // ── New domain entities ────────────────────────────────────────────

    /// <summary>A subject area offered by the school (e.g. Mathematics Grade 10).</summary>
    public class Course
    {
        /// <summary>Primary key for the course record.</summary>
        public int Id { get; set; }

        /// <summary>Human-readable course name.</summary>
        public string Name { get; set; }

        /// <summary>Course code used for registration.</summary>
        public string Code { get; set; }

        /// <summary>Detailed description of the course.</summary>
        public string Description { get; set; }

        /// <summary>UTC timestamp when the course was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp when the course was last updated.</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Flag indicating whether this course has been soft-deleted.</summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>Class groups offered under this course.</summary>
        public virtual ICollection<ClassGroup> ClassGroups { get; set; }
    }

    /// <summary>
    /// A class taught by one teacher within a course
    /// (e.g. Grade 10A — Mathematics, taught by Mrs Smith).
    /// </summary>
    public class ClassGroup
    {
        /// <summary>Primary key for the class group.</summary>
        public int Id { get; set; }

        /// <summary>Class group name.</summary>
        public string Name { get; set; }

        /// <summary>Academic grade level for this class group.</summary>
        public int GradeLevel { get; set; }

        /// <summary>Identifier of the course for this class group.</summary>
        public int CourseId { get; set; }

        /// <summary>Identifier of the teacher assigned to this class group.</summary>
        public int TeacherId { get; set; }

        /// <summary>UTC timestamp when the class group was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp when the class group was last updated.</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Flag indicating whether this class group has been soft-deleted.</summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>Course associated with this class group.</summary>
        public virtual Course Course { get; set; }

        /// <summary>Teacher assigned to this class group.</summary>
        public virtual Teacher Teacher { get; set; }

        /// <summary>Student enrollments for this class group.</summary>
        public virtual ICollection<StudentEnrollment> Enrollments { get; set; }

        /// <summary>Assignments created for this class group.</summary>
        public virtual ICollection<Assignment> Assignments { get; set; }
    }

    /// <summary>Junction table — a student enrolled in a class.</summary>
    public class StudentEnrollment
    {
        /// <summary>Primary key for the enrollment record.</summary>
        public int Id { get; set; }

        /// <summary>Identifier of the enrolled student.</summary>
        public int StudentId { get; set; }

        /// <summary>Identifier of the class group the student is enrolled in.</summary>
        public int ClassGroupId { get; set; }

        /// <summary>UTC timestamp when the student enrolled.</summary>
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp when the enrollment was last updated.</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Flag indicating whether this enrollment has been soft-deleted.</summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>Student linked to this enrollment.</summary>
        public virtual Student Student { get; set; }

        /// <summary>Class group linked to this enrollment.</summary>
        public virtual ClassGroup ClassGroup { get; set; }
    }

    /// <summary>A task/assignment created by a teacher for one of their classes.</summary>
    public class Assignment
    {
        /// <summary>Primary key for the assignment.</summary>
        public int Id { get; set; }

        /// <summary>Assignment title.</summary>
        public string Title { get; set; }

        /// <summary>Assignment instructions or description.</summary>
        public string Description { get; set; }

        /// <summary>Due date for the assignment.</summary>
        public DateTime DueDate { get; set; }

        /// <summary>Maximum available score for the assignment.</summary>
        public int MaxScore { get; set; }

        /// <summary>Class group the assignment belongs to.</summary>
        public int ClassGroupId { get; set; }

        /// <summary>Teacher who created the assignment.</summary>
        public int CreatedByTeacherId { get; set; }

        /// <summary>UTC timestamp when the assignment was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        /// <summary>UTC timestamp when the assignment was last updated (used for optimistic concurrency).</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Flag indicating whether this assignment has been soft-deleted.</summary>
        public bool IsDeleted { get; set; } = false;
        /// <summary>Class group navigation property.</summary>
        public virtual ClassGroup ClassGroup { get; set; }

        /// <summary>Teacher who created the assignment.</summary>
        public virtual Teacher CreatedBy { get; set; }

        /// <summary>Student submissions for this assignment.</summary>
        public virtual ICollection<AssignmentSubmission> Submissions { get; set; }
    }

    /// <summary>
    /// A student's submitted answer for an assignment.
    /// Score is null until the teacher grades it.
    /// </summary>
    public class AssignmentSubmission
    {
        /// <summary>Primary key for the submission.</summary>
        public int Id { get; set; }

        /// <summary>Identifier of the related assignment.</summary>
        public int AssignmentId { get; set; }

        /// <summary>Identifier of the student who submitted.</summary>
        public int StudentId { get; set; }

        /// <summary>UTC timestamp when the submission was made.</summary>
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp when the submission was last updated (used for optimistic concurrency).</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Submission content or answer body.</summary>
        public string Content { get; set; }

        /// <summary>Score awarded by the teacher.</summary>
        public int? Score { get; set; }

        /// <summary>Teacher feedback for the submission.</summary>
        public string Feedback { get; set; }

        /// <summary>Status of the submission.</summary>
        public string Status { get; set; } = SubmissionStatus.Pending;

        /// <summary>Flag indicating whether this submission has been soft-deleted.</summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>Assignment associated with this submission.</summary>
        public virtual Assignment Assignment { get; set; }

        /// <summary>Student associated with this submission.</summary>
        public virtual Student Student { get; set; }
    }

    /// <summary>Allowed values for AssignmentSubmission.Status.</summary>
    public static class SubmissionStatus
    {
        /// <summary>Submission waiting to be graded.</summary>
        public const string Pending = "Pending";

        /// <summary>Submission has been graded.</summary>
        public const string Graded = "Graded";

        /// <summary>Submission was graded after the due date.</summary>
        public const string Late = "Late";
    }

    /// <summary>
    /// Immutable audit log for all system changes.
    /// Enables administrative oversight and compliance tracking.
    /// </summary>
    public class AuditLog
    {
        /// <summary>Primary key for the audit record.</summary>
        public int Id { get; set; }

        /// <summary>Action taken on the entity.</summary>
        public string Action { get; set; }

        /// <summary>Type of entity affected by the action.</summary>
        public string EntityType { get; set; }

        /// <summary>Identifier of the affected entity.</summary>
        public int EntityId { get; set; }

        /// <summary>JSON describing the change details.</summary>
        public string Changes { get; set; }

        /// <summary>Identifier of who performed the action.</summary>
        public string PerformedBy { get; set; }

        /// <summary>UTC timestamp when the action was performed.</summary>
        public DateTime PerformedAt { get; set; }

        /// <summary>IP address of the requester, if available.</summary>
        public string IpAddress { get; set; }

        /// <summary>User agent string of the requester, if available.</summary>
        public string UserAgent { get; set; }
    }

    /// <summary>Administrative user with system access.</summary>
    public class Admin
    {
        /// <summary>Primary key for the admin account.</summary>
        public int Id { get; set; }

        /// <summary>Administrator given name.</summary>
        public string FirstName { get; set; }

        /// <summary>Administrator family name.</summary>
        public string LastName { get; set; }

        /// <summary>Administrator email address used for login.</summary>
        public string Email { get; set; }

        /// <summary>Administrator contact phone number.</summary>
        public string Phone { get; set; }

        /// <summary>BCrypt hash of the administrator password.</summary>
        public string Password { get; set; }

        /// <summary>UTC timestamp when the admin account was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp when the admin account was last updated.</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
