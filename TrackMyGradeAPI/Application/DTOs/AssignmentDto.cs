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
        /// <summary>Gets or sets the user ID.</summary>
        public int    UserId    { get; set; }
        /// <summary>Gets or sets the user's full name.</summary>
        public string FullName  { get; set; }
        /// <summary>Gets or sets the user's email address.</summary>
        public string Email     { get; set; }
        /// <summary>Gets or sets the user's role (Teacher or Student).</summary>
        public string Role      { get; set; }
        /// <summary>Gets or sets the JWT authentication token.</summary>
        public string Token     { get; set; }
        /// <summary>Gets or sets the Angular route to redirect to after login.</summary>
        public string Dashboard { get; set; }
    }

    /// <summary>Pre-activation status check — used to display the user's name before they set a password.</summary>
    public class ActivationStatusDto
    {
        /// <summary>Gets or sets a value indicating whether the account is activated.</summary>
        public bool      IsActivated { get; set; }
        /// <summary>Gets or sets the user's full name.</summary>
        public string    FullName    { get; set; }
        /// <summary>Gets or sets the user's email address.</summary>
        public string    Email       { get; set; }
        /// <summary>Gets or sets the user's role (Teacher or Student).</summary>
        public string    Role        { get; set; }
    }

    // ── Assignments ───────────────────────────────────────────────────────

    /// <summary>Request body for creating a new assignment (teacher only).</summary>
    public class AssignmentCreateDto
    {
        /// <summary>Gets or sets the assignment title.</summary>
        public string   Title        { get; set; }
        /// <summary>Gets or sets the assignment description.</summary>
        public string   Description  { get; set; }
        /// <summary>Gets or sets the assignment due date.</summary>
        public DateTime DueDate      { get; set; }
        /// <summary>Gets or sets the maximum score for the assignment.</summary>
        public int      MaxScore     { get; set; }
        /// <summary>Gets or sets the class group ID this assignment is for.</summary>
        public int      ClassGroupId { get; set; }
    }

    /// <summary>Assignment details returned to teachers and students.</summary>
    public class AssignmentResponseDto
    {
        /// <summary>Gets or sets the assignment ID.</summary>
        public int      Id                      { get; set; }
        /// <summary>Gets or sets the assignment title.</summary>
        public string   Title                   { get; set; }
        /// <summary>Gets or sets the assignment description.</summary>
        public string   Description             { get; set; }
        /// <summary>Gets or sets the assignment due date.</summary>
        public DateTime DueDate                 { get; set; }
        /// <summary>Gets or sets the maximum score for the assignment.</summary>
        public int      MaxScore                { get; set; }
        /// <summary>Gets or sets the class group ID.</summary>
        public int      ClassGroupId            { get; set; }
        /// <summary>Gets or sets the class group name.</summary>
        public string   ClassGroupName          { get; set; }
        /// <summary>Gets or sets the date and time when the assignment was created.</summary>
        public DateTime CreatedAt               { get; set; }
        /// <summary>Gets or sets the number of submissions received (teacher view).</summary>
        public int      SubmissionCount         { get; set; }
        /// <summary>Gets or sets the student's own submission status (student view).</summary>
        public string   StudentSubmissionStatus { get; set; }
    }

    // ── Submissions ───────────────────────────────────────────────────────

    /// <summary>Request body for a student submitting their assignment work.</summary>
    public class SubmissionCreateDto
    {
        /// <summary>Gets or sets the student's text answer. File uploads can be handled separately.</summary>
        public string Content { get; set; }
    }

    /// <summary>Full submission record returned to teacher or student.</summary>
    public class SubmissionResponseDto
    {
        /// <summary>Gets or sets the submission ID.</summary>
        public int      Id              { get; set; }
        /// <summary>Gets or sets the assignment ID.</summary>
        public int      AssignmentId    { get; set; }
        /// <summary>Gets or sets the assignment title.</summary>
        public string   AssignmentTitle { get; set; }
        /// <summary>Gets or sets the student ID.</summary>
        public int      StudentId       { get; set; }
        /// <summary>Gets or sets the student name.</summary>
        public string   StudentName     { get; set; }
        /// <summary>Gets or sets the date and time when the submission was made.</summary>
        public DateTime SubmittedAt     { get; set; }
        /// <summary>Gets or sets the submission content.</summary>
        public string   Content         { get; set; }
        /// <summary>Gets or sets the score given by the teacher (null until graded).</summary>
        public int?     Score           { get; set; }
        /// <summary>Gets or sets the maximum score for this assignment.</summary>
        public int      MaxScore        { get; set; }
        /// <summary>Gets or sets the feedback from the teacher.</summary>
        public string   Feedback        { get; set; }
        /// <summary>Gets or sets the submission status (Pending | Graded | Late).</summary>
        public string   Status          { get; set; }
    }

    /// <summary>Request body for a teacher grading a submission.</summary>
    public class GradingDto
    {
        /// <summary>Gets or sets the score to assign to the submission.</summary>
        public int    Score    { get; set; }
        /// <summary>Gets or sets the feedback text from the teacher.</summary>
        public string Feedback { get; set; }
    }
}
