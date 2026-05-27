using System;

namespace TrackMyGradeAPI.DTOs
{
    /// <summary>Response returned after a successful administrator login.</summary>
    public class AdminResponseDto
    {
        /// <summary>Gets or sets the admin's unique identifier.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the admin's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Gets or sets the admin's last name.</summary>
        public string LastName { get; set; }
        /// <summary>Gets or sets the admin's email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the admin's phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Gets or sets the JWT authentication token.</summary>
        public string Token { get; set; }
    }

    /// <summary>Request body for administrator login.</summary>
    public class AdminLoginDto
    {
        /// <summary>Gets or sets the administrator email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the administrator password.</summary>
        public string Password { get; set; }
    }

    /// <summary>Teacher profile data as viewed from the admin dashboard.</summary>
    public class AdminTeacherDto
    {
        /// <summary>Gets or sets the teacher ID.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the teacher's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Gets or sets the teacher's last name.</summary>
        public string LastName { get; set; }
        /// <summary>Gets or sets the teacher's email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the teacher's phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Gets or sets the subject assigned to the teacher.</summary>
        public string Subject { get; set; }
        /// <summary>Gets or sets a value indicating whether the account has been activated.</summary>
        public bool IsActivated { get; set; }
        /// <summary>Gets or sets the one-time token for account activation.</summary>
        public string ActivationToken { get; set; }
        /// <summary>Gets or sets the date and time when the account was activated.</summary>
        public DateTime? ActivatedAt { get; set; }
    }

    /// <summary>Data required to create a new teacher account by an administrator.</summary>
    public class AdminCreateTeacherDto
    {
        /// <summary>Gets or sets the first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Gets or sets the last name.</summary>
        public string LastName { get; set; }
        /// <summary>Gets or sets the unique email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the contact phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Gets or sets the subject they will teach.</summary>
        public string Subject { get; set; }
    }

    /// <summary>Student profile data as viewed from the admin dashboard.</summary>
    public class AdminStudentDto
    {
        /// <summary>Gets or sets the student ID.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the system-generated student number.</summary>
        public string StudentNumber { get; set; }
        /// <summary>Gets or sets the first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Gets or sets the last name.</summary>
        public string LastName { get; set; }
        /// <summary>Gets or sets the unique email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the contact phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Gets or sets the OMANG or passport number.</summary>
        public string OmangOrPassport { get; set; }
        /// <summary>Gets or sets the grade level.</summary>
        public int Grade { get; set; }
        /// <summary>Gets or sets a value indicating whether the account is activated.</summary>
        public bool IsActivated { get; set; }
        /// <summary>Gets or sets the activation token.</summary>
        public string ActivationToken { get; set; }
        /// <summary>Gets or sets the activation date.</summary>
        public DateTime? ActivatedAt { get; set; }
        /// <summary>Gets or sets the ID of the assigned primary teacher.</summary>
        public int TeacherId { get; set; }
    }

    /// <summary>Data required to create a new student account.</summary>
    public class AdminCreateStudentDto
    {
        /// <summary>Gets or sets the first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Gets or sets the last name.</summary>
        public string LastName { get; set; }
        /// <summary>Gets or sets the unique email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the contact phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Gets or sets the identity document number.</summary>
        public string OmangOrPassport { get; set; }
        /// <summary>Gets or sets the academic grade.</summary>
        public int Grade { get; set; }
        /// <summary>Gets or sets the ID of the primary teacher.</summary>
        public int TeacherId { get; set; }
    }

    /// <summary>Data required to update an existing student record.</summary>
    public class AdminUpdateStudentDto : AdminCreateStudentDto { }
}