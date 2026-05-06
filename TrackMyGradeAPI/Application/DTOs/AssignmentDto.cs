using System;

namespace TrackMyGradeAPI.DTOs
{
    // ── Account activation ────────────────────────────────────────────────

    /// <summary>Request to activate a new teacher or student account and set a password.</summary>
    public class ActivateAccountDto
    {
        /// <summary>Must be "Teacher" or "Student".</summary>
        public string Role            { get; set; }
        /// <summary>The one-time token provided by the admin.</summary>
        public string ActivationToken { get; set; }
        /// <summary>Chosen password (minimum 8 characters).</summary>
        public string NewPassword     { get; set; }
        /// <summary>Must match NewPassword.</summary>
        public string ConfirmPassword { get; set; }
    }

    /// <summary>Response after successful activation — includes JWT for immediate login.</summary>
    public class ActivationResponseDto
    {
        public int    UserId    { get; set; }
        public string FullName  { get; set; }
        public string Email     { get; set; }
        public string Role      { get; set; }
        public string Token     { get; set; }
        public string Dashboard { get; set; }  // Angular route to redirect to
    }

    /// <summary>Pre-activation status check — used to display the user's name before they set a password.</summary>
    public class ActivationStatusDto
    {
        public bool      IsActivated { get; set; }
        public string    FullName    { get; set; }
        public string    Email       { get; set; }
        public string    Role        { get; set; }
    }

    // ── Assignments ───────────────────────────────────────────────────────

    /// <summary>Request body for creating a new assignment (teacher only).</summary>
    public class AssignmentCreateDto
    {
        public string   Title        { get; set; }
        public string   Description  { get; set; }
        public DateTime DueDate      { get; set; }
        public int      MaxScore     { get; set; }
        public int      ClassGroupId { get; set; }
    }

    /// <summary>Assignment details returned to teachers and students.</summary>
    public class AssignmentResponseDto
    {
        public int      Id                      { get; set; }
        public string   Title                   { get; set; }
        public string   Description             { get; set; }
        public DateTime DueDate                 { get; set; }
        public int      MaxScore                { get; set; }
        public int      ClassGroupId            { get; set; }
        public string   ClassGroupName          { get; set; }
        public DateTime CreatedAt               { get; set; }
        /// <summary>Number of submissions received (teacher view).</summary>
        public int      SubmissionCount         { get; set; }
        /// <summary>Student's own submission status (student view).</summary>
        public string   StudentSubmissionStatus { get; set; }
    }

    // ── Submissions ───────────────────────────────────────────────────────

    /// <summary>Request body for a student submitting their assignment work.</summary>
    public class SubmissionCreateDto
    {
        /// <summary>Student's text answer. File uploads can be handled separately.</summary>
        public string Content { get; set; }
    }

    /// <summary>Full submission record returned to teacher or student.</summary>
    public class SubmissionResponseDto
    {
        public int      Id              { get; set; }
        public int      AssignmentId    { get; set; }
        public string   AssignmentTitle { get; set; }
        public int      StudentId       { get; set; }
        public string   StudentName     { get; set; }
        public DateTime SubmittedAt     { get; set; }
        public string   Content         { get; set; }
        /// <summary>Null until teacher grades.</summary>
        public int?     Score           { get; set; }
        public int      MaxScore        { get; set; }
        public string   Feedback        { get; set; }
        /// <summary>Pending | Graded | Late</summary>
        public string   Status          { get; set; }
    }

    /// <summary>Request body for a teacher grading a submission.</summary>
    public class GradingDto
    {
        public int    Score    { get; set; }
        public string Feedback { get; set; }
    }
}
