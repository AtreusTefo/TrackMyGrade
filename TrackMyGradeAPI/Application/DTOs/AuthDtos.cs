using System;

namespace TrackMyGradeAPI.DTOs
{
    /// <summary>Request body for student authentication.</summary>
    public class StudentLoginDto
    {
        /// <summary>Gets or sets the student email.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the student password.</summary>
        public string Password { get; set; }
    }

    /// <summary>Auth response for a student containing profile and token.</summary>
    public class StudentAuthResponseDto
    {
        /// <summary>Gets or sets the student ID.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Gets or sets the last name.</summary>
        public string LastName { get; set; }
        /// <summary>Gets or sets the email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the session JWT token.</summary>
        public string Token { get; set; }
    }

    /// <summary>Request body for teacher authentication.</summary>
    public class TeacherLoginDto
    {
        /// <summary>Gets or sets the teacher email.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the teacher password.</summary>
        public string Password { get; set; }
    }

    /// <summary>Auth response for a teacher containing profile and token.</summary>
    public class TeacherResponseDto
    {
        /// <summary>Gets or sets the teacher ID.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Gets or sets the last name.</summary>
        public string LastName { get; set; }
        /// <summary>Gets or sets the email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the session JWT token.</summary>
        public string Token { get; set; }
    }

    /// <summary>Data required to activate a pending account and set a password.</summary>
    public class ActivateAccountDto
    {
        /// <summary>Gets or sets the role (Teacher or Student).</summary>
        public string Role { get; set; }
        /// <summary>Gets or sets the secret activation token.</summary>
        public string ActivationToken { get; set; }
        /// <summary>Gets or sets the new password.</summary>
        public string NewPassword { get; set; }
        /// <summary>Must match NewPassword.</summary>
        public string ConfirmPassword { get; set; }
    }

    /// <summary>Response returned after successful account activation.</summary>
    public class ActivationResponseDto
    {
        /// <summary>Gets or sets the primary key of the activated user.</summary>
        public int UserId { get; set; }
        /// <summary>Gets or sets the full name.</summary>
        public string FullName { get; set; }
        /// <summary>Gets or sets the email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the assigned role.</summary>
        public string Role { get; set; }
        /// <summary>Gets or sets the initial JWT token.</summary>
        public string Token { get; set; }
        /// <summary>Gets or sets the path to the user's dashboard.</summary>
        public string Dashboard { get; set; }
    }

    /// <summary>Status information for a user pending activation.</summary>
    public class ActivationStatusDto
    {
        /// <summary>Gets or sets a value indicating whether the account is already activated.</summary>
        public bool IsActivated { get; set; }
        /// <summary>Gets or sets the full name of the user.</summary>
        public string FullName { get; set; }
        /// <summary>Gets or sets the email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the user's role.</summary>
        public string Role { get; set; }
    }

    /// <summary>Detailed view of a student including academic performance metrics.</summary>
    public class StudentResponseDto
    {
        /// <summary>Gets or sets the student identifier.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the system-generated student number.</summary>
        public string StudentNumber { get; set; }
        /// <summary>Gets or sets the first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Gets or sets the last name.</summary>
        public string LastName { get; set; }
        /// <summary>Gets or sets the email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the contact phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Gets or sets the academic grade.</summary>
        public int Grade { get; set; }
        /// <summary>Gets or sets Assessment 1 score (0-20).</summary>
        public int Assessment1 { get; set; }
        /// <summary>Gets or sets Assessment 2 score (0-20).</summary>
        public int Assessment2 { get; set; }
        /// <summary>Gets or sets Assessment 3 score (0-20).</summary>
        public int Assessment3 { get; set; }
        /// <summary>Gets or sets the sum of all assessments.</summary>
        public int Total { get; set; }
        /// <summary>Gets or sets the average score.</summary>
        public double Average { get; set; }
        /// <summary>Gets or sets the percentage (out of 100).</summary>
        public double Percentage { get; set; }
        /// <summary>Gets or sets the performance level (e.g., Excellent, Good).</summary>
        public string PerformanceLevel { get; set; }
    }
}