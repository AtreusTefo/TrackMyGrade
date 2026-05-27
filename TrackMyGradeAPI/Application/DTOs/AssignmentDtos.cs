using System;

namespace TrackMyGradeAPI.DTOs
{
    /// <summary>Detailed view of an assignment for teachers and students.</summary>
    public class AssignmentResponseDto
    {
        /// <summary>Gets or sets the assignment ID.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the title.</summary>
        public string Title { get; set; }
        /// <summary>Gets or sets the description.</summary>
        public string Description { get; set; }
        /// <summary>Gets or sets the submission deadline.</summary>
        public DateTime DueDate { get; set; }
        /// <summary>Gets or sets the maximum achievable score.</summary>
        public int MaxScore { get; set; }
        /// <summary>Gets or sets the class group ID.</summary>
        public int ClassGroupId { get; set; }
        /// <summary>Gets or sets the name of the class group.</summary>
        public string ClassGroupName { get; set; }
        /// <summary>Gets or sets the creation date.</summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>Gets or sets the count of received submissions.</summary>
        public int SubmissionCount { get; set; }
        /// <summary>Gets or sets the current student's submission status.</summary>
        public string StudentSubmissionStatus { get; set; }
    }

    /// <summary>Request body for a teacher creating a new assignment.</summary>
    public class AssignmentCreateDto
    {
        /// <summary>Gets or sets the title.</summary>
        public string Title { get; set; }
        /// <summary>Gets or sets the description.</summary>
        public string Description { get; set; }
        /// <summary>Gets or sets the due date.</summary>
        public DateTime DueDate { get; set; }
        /// <summary>Gets or sets the maximum score.</summary>
        public int MaxScore { get; set; }
        /// <summary>Gets or sets the target class group ID.</summary>
        public int ClassGroupId { get; set; }
    }

    /// <summary>Details of a student's submission for an assignment.</summary>
    public class SubmissionResponseDto
    {
        /// <summary>Gets or sets the submission ID.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the associated assignment ID.</summary>
        public int AssignmentId { get; set; }
        /// <summary>Gets or sets the title of the assignment.</summary>
        public string AssignmentTitle { get; set; }
        /// <summary>Gets or sets the student ID.</summary>
        public int StudentId { get; set; }
        /// <summary>Gets or sets the full name of the student.</summary>
        public string StudentName { get; set; }
        /// <summary>Gets or sets the submission date.</summary>
        public DateTime SubmittedAt { get; set; }
        /// <summary>Gets or sets the text content of the submission.</summary>
        public string Content { get; set; }
        /// <summary>Gets or sets the assigned score.</summary>
        public int? Score { get; set; }
        /// <summary>Gets or sets the max score of the assignment.</summary>
        public int MaxScore { get; set; }
        /// <summary>Gets or sets the teacher's feedback.</summary>
        public string Feedback { get; set; }
        /// <summary>Gets or sets the submission status (e.g., Graded, Pending).</summary>
        public string Status { get; set; }
    }

    /// <summary>Request body for a student submitting assignment work.</summary>
    public class SubmissionCreateDto
    {
        /// <summary>Gets or sets the content of the assignment work.</summary>
        public string Content { get; set; }
    }

    /// <summary>Data transfer object for grading a submission.</summary>
    public class GradingDto
    {
        /// <summary>Gets or sets the score.</summary>
        public int Score { get; set; }
        /// <summary>Gets or sets the feedback comments.</summary>
        public string Feedback { get; set; }
    }
}