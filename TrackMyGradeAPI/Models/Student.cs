using System;
using System.Collections.Generic;

namespace TrackMyGradeAPI.Models
{
    // ── Existing models (Teacher & Student) updated below ─────────────

    public class Teacher
    {
        public int    Id           { get; set; }
        public string FirstName    { get; set; }
        public string LastName     { get; set; }
        public string Email        { get; set; }
        public string Phone        { get; set; }
        public string Subject      { get; set; }
        public string PasswordHash { get; set; }   // BCrypt hash — replaces plain-text Password
        public string Token        { get; set; }   // Session GUID (kept for backward compat)

        // Account activation
        public bool      IsActivated     { get; set; } = false;
        public string    ActivationToken { get; set; }
        public DateTime? ActivatedAt     { get; set; }

        // Navigation
        public virtual ICollection<ClassGroup>  ClassGroups  { get; set; }
        public virtual ICollection<Student>     Students     { get; set; }
        public virtual ICollection<Assignment>  Assignments  { get; set; }
    }

    public class Student
    {
        public int    Id                { get; set; }
        public string StudentNumber     { get; set; }   // e.g. STU-2026-0001
        public int    TeacherId         { get; set; }   // primary teacher (kept for legacy reads)
        public string FirstName         { get; set; }
        public string LastName          { get; set; }
        public string Email             { get; set; }
        public string Phone             { get; set; }
        public string OmangOrPassport   { get; set; }
        public int    Grade             { get; set; }
        public string PasswordHash      { get; set; }   // BCrypt hash
        public string Token             { get; set; }   // Session GUID

        // Account activation
        public bool      IsActivated     { get; set; } = false;
        public string    ActivationToken { get; set; }
        public DateTime? ActivatedAt     { get; set; }

        // Navigation
        public virtual Teacher                          Teacher     { get; set; }
        public virtual ICollection<StudentEnrollment>   Enrollments { get; set; }
        public virtual ICollection<AssignmentSubmission> Submissions { get; set; }
    }

    // ── New domain entities ────────────────────────────────────────────

    /// <summary>A subject area offered by the school (e.g. Mathematics Grade 10).</summary>
    public class Course
    {
        public int    Id          { get; set; }
        public string Name        { get; set; }
        public string Code        { get; set; }   // e.g. MATH-10
        public string Description { get; set; }

        public virtual ICollection<ClassGroup> ClassGroups { get; set; }
    }

    /// <summary>
    /// A class taught by one teacher within a course
    /// (e.g. Grade 10A — Mathematics, taught by Mrs Smith).
    /// </summary>
    public class ClassGroup
    {
        public int    Id         { get; set; }
        public string Name       { get; set; }   // e.g. 10A
        public int    GradeLevel { get; set; }   // 7–12
        public int    CourseId   { get; set; }
        public int    TeacherId  { get; set; }

        public virtual Course                          Course      { get; set; }
        public virtual Teacher                         Teacher     { get; set; }
        public virtual ICollection<StudentEnrollment>  Enrollments { get; set; }
        public virtual ICollection<Assignment>         Assignments { get; set; }
    }

    /// <summary>Junction table — a student enrolled in a class.</summary>
    public class StudentEnrollment
    {
        public int      Id           { get; set; }
        public int      StudentId    { get; set; }
        public int      ClassGroupId { get; set; }
        public DateTime EnrolledAt   { get; set; } = DateTime.UtcNow;

        public virtual Student    Student    { get; set; }
        public virtual ClassGroup ClassGroup { get; set; }
    }

    /// <summary>A task/assignment created by a teacher for one of their classes.</summary>
    public class Assignment
    {
        public int      Id                  { get; set; }
        public string   Title               { get; set; }
        public string   Description         { get; set; }
        public DateTime DueDate             { get; set; }
        public int      MaxScore            { get; set; }
        public int      ClassGroupId        { get; set; }
        public int      CreatedByTeacherId  { get; set; }
        public DateTime CreatedAt           { get; set; } = DateTime.UtcNow;

        public virtual ClassGroup                       ClassGroup   { get; set; }
        public virtual Teacher                          CreatedBy    { get; set; }
        public virtual ICollection<AssignmentSubmission> Submissions { get; set; }
    }

    /// <summary>
    /// A student's submitted answer for an assignment.
    /// Score is null until the teacher grades it.
    /// </summary>
    public class AssignmentSubmission
    {
        public int      Id           { get; set; }
        public int      AssignmentId { get; set; }
        public int      StudentId    { get; set; }
        public DateTime SubmittedAt  { get; set; } = DateTime.UtcNow;
        public string   Content      { get; set; }   // text answer / file path
        public int?     Score        { get; set; }   // null = not yet graded
        public string   Feedback     { get; set; }
        public string   Status       { get; set; } = SubmissionStatus.Pending;

        public virtual Assignment Assignment { get; set; }
        public virtual Student    Student    { get; set; }
    }

    /// <summary>Allowed values for AssignmentSubmission.Status.</summary>
    public static class SubmissionStatus
    {
        public const string Pending = "Pending";
        public const string Graded  = "Graded";
        public const string Late    = "Late";
    }

    /// <summary>
    /// Immutable audit log for all system changes.
    /// Enables administrative oversight and compliance tracking.
    /// </summary>
    public class AuditLog
    {
        public int      Id              { get; set; }
        public string   Action          { get; set; }      // Created, Updated, Deleted
        public string   EntityType      { get; set; }      // Teacher, Student, Assignment, etc.
        public int      EntityId        { get; set; }      // ID of the affected entity
        public string   Changes         { get; set; }      // JSON serialized before/after values
        public string   PerformedBy     { get; set; }      // Admin email or system identifier
        public DateTime PerformedAt     { get; set; }      // UTC timestamp
        public string   IpAddress       { get; set; }      // Optional: requester IP
        public string   UserAgent       { get; set; }      // Optional: requester user agent
    }
}
