using System;

namespace TrackMyGradeAPI.DTOs
{
    // ── Admin auth ────────────────────────────────────────────────────────

    /// <summary>Admin login request.</summary>
    public class AdminLoginDto
    {
        /// <summary>Gets or sets the admin email address.</summary>
        public string Email    { get; set; }
        /// <summary>Gets or sets the admin password.</summary>
        public string Password { get; set; }
    }

    /// <summary>Admin login response — returns admin profile with JWT token.</summary>
    public class AdminResponseDto
    {
        /// <summary>Gets or sets the admin ID.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the admin first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Gets or sets the admin last name.</summary>
        public string LastName { get; set; }
        /// <summary>Gets or sets the admin email address.</summary>
        public string Email { get; set; }
        /// <summary>Gets or sets the admin phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Gets or sets the JWT authentication token.</summary>
        public string Token { get; set; }
    }

    // ── Admin → Teacher management ────────────────────────────────────────

    /// <summary>Request to create a new teacher account (admin only). No password — activation flow sets it.</summary>
    public class AdminCreateTeacherDto
    {
        /// <summary>Gets or sets the teacher first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Gets or sets the teacher last name.</summary>
        public string LastName  { get; set; }
        /// <summary>Gets or sets the teacher email address.</summary>
        public string Email     { get; set; }
        /// <summary>Gets or sets the teacher phone number.</summary>
        public string Phone     { get; set; }
        /// <summary>Gets or sets the subject taught by the teacher.</summary>
        public string Subject   { get; set; }
    }

    /// <summary>Teacher record as seen by the admin dashboard.</summary>
    public class AdminTeacherDto
    {
        /// <summary>Gets or sets the teacher ID.</summary>
        public int       Id              { get; set; }
        /// <summary>Gets or sets the teacher first name.</summary>
        public string    FirstName       { get; set; }
        /// <summary>Gets or sets the teacher last name.</summary>
        public string    LastName        { get; set; }
        /// <summary>Gets or sets the teacher email address.</summary>
        public string    Email           { get; set; }
        /// <summary>Gets or sets the teacher phone number.</summary>
        public string    Phone           { get; set; }
        /// <summary>Gets or sets the subject taught by the teacher.</summary>
        public string    Subject         { get; set; }
        /// <summary>Gets or sets a value indicating whether the teacher account is activated.</summary>
        public bool      IsActivated     { get; set; }
        /// <summary>Gets or sets the activation token. Returned only for unactivated accounts so admin can share the link.</summary>
        public string    ActivationToken { get; set; }
        /// <summary>Gets or sets the date and time when the account was activated.</summary>
        public DateTime? ActivatedAt     { get; set; }
    }

    // ── Admin → Student management ────────────────────────────────────────

    /// <summary>Request to create a new student (admin only). No password — activation flow sets it.</summary>
    public class AdminCreateStudentDto
    {
        /// <summary>Gets or sets the student first name.</summary>
        public string FirstName       { get; set; }
        /// <summary>Gets or sets the student last name.</summary>
        public string LastName        { get; set; }
        /// <summary>Gets or sets the student email address.</summary>
        public string Email           { get; set; }
        /// <summary>Gets or sets the student phone number.</summary>
        public string Phone           { get; set; }
        /// <summary>Gets or sets the student OMANG or passport number.</summary>
        public string OmangOrPassport { get; set; }
        /// <summary>Gets or sets the grade/class level of the student.</summary>
        public int    Grade           { get; set; }
        /// <summary>Gets or sets the primary teacher ID (for legacy student list).</summary>
        public int    TeacherId       { get; set; }
    }

    /// <summary>Request to update a student's personal details (admin only).</summary>
    public class AdminUpdateStudentDto
    {
        /// <summary>Gets or sets the student first name.</summary>
        public string FirstName       { get; set; }
        /// <summary>Gets or sets the student last name.</summary>
        public string LastName        { get; set; }
        /// <summary>Gets or sets the student email address.</summary>
        public string Email           { get; set; }
        /// <summary>Gets or sets the student phone number.</summary>
        public string Phone           { get; set; }
        /// <summary>Gets or sets the student OMANG or passport number.</summary>
        public string OmangOrPassport { get; set; }
        /// <summary>Gets or sets the grade/class level of the student.</summary>
        public int    Grade           { get; set; }
        /// <summary>Gets or sets the primary teacher ID.</summary>
        public int    TeacherId       { get; set; }
    }

    /// <summary>Student record as seen by the admin dashboard.</summary>
    public class AdminStudentDto
    {
        /// <summary>Gets or sets the student ID.</summary>
        public int       Id              { get; set; }
        /// <summary>Gets or sets the student number/registration ID.</summary>
        public string    StudentNumber   { get; set; }
        /// <summary>Gets or sets the student first name.</summary>
        public string    FirstName       { get; set; }
        /// <summary>Gets or sets the student last name.</summary>
        public string    LastName        { get; set; }
        /// <summary>Gets or sets the student email address.</summary>
        public string    Email           { get; set; }
        /// <summary>Gets or sets the student phone number.</summary>
        public string    Phone           { get; set; }
        /// <summary>Gets or sets the student OMANG or passport number.</summary>
        public string    OmangOrPassport { get; set; }
        /// <summary>Gets or sets the grade/class level of the student.</summary>
        public int       Grade           { get; set; }
        /// <summary>Gets or sets the primary teacher ID.</summary>
        public int       TeacherId       { get; set; }
        /// <summary>Gets or sets a value indicating whether the student account is activated.</summary>
        public bool      IsActivated     { get; set; }
        /// <summary>Gets or sets the activation token. Returned only for unactivated accounts.</summary>
        public string    ActivationToken { get; set; }
        /// <summary>Gets or sets the date and time when the account was activated.</summary>
        public DateTime? ActivatedAt     { get; set; }
    }

    // ── Admin → Course / ClassGroup ───────────────────────────────────────

    /// <summary>Request to create a new course.</summary>
    public class CreateCourseDto
    {
        /// <summary>Gets or sets the course name.</summary>
        public string Name        { get; set; }
        /// <summary>Gets or sets the course code.</summary>
        public string Code        { get; set; }
        /// <summary>Gets or sets the course description.</summary>
        public string Description { get; set; }
    }

    /// <summary>Course data transfer object.</summary>
    public class CourseDto
    {
        /// <summary>Gets or sets the course ID.</summary>
        public int    Id          { get; set; }
        /// <summary>Gets or sets the course name.</summary>
        public string Name        { get; set; }
        /// <summary>Gets or sets the course code.</summary>
        public string Code        { get; set; }
        /// <summary>Gets or sets the course description.</summary>
        public string Description { get; set; }
    }

    /// <summary>Request to create a new class group.</summary>
    public class CreateClassGroupDto
    {
        /// <summary>Gets or sets the class group name.</summary>
        public string Name       { get; set; }
        /// <summary>Gets or sets the grade level of the class.</summary>
        public int    GradeLevel { get; set; }
        /// <summary>Gets or sets the course ID.</summary>
        public int    CourseId   { get; set; }
        /// <summary>Gets or sets the teacher ID assigned to the class.</summary>
        public int    TeacherId  { get; set; }
    }

    /// <summary>Class group data transfer object.</summary>
    public class ClassGroupDto
    {
        /// <summary>Gets or sets the class group ID.</summary>
        public int    Id          { get; set; }
        /// <summary>Gets or sets the class group name.</summary>
        public string Name        { get; set; }
        /// <summary>Gets or sets the grade level of the class.</summary>
        public int    GradeLevel  { get; set; }
        /// <summary>Gets or sets the course ID.</summary>
        public int    CourseId    { get; set; }
        /// <summary>Gets or sets the course name.</summary>
        public string CourseName  { get; set; }
        /// <summary>Gets or sets the teacher ID assigned to the class.</summary>
        public int    TeacherId   { get; set; }
        /// <summary>Gets or sets the teacher name.</summary>
        public string TeacherName { get; set; }
    }

    /// <summary>Request to enroll a student in a class group.</summary>
    public class EnrollStudentDto
    {
        /// <summary>Gets or sets the student ID to enroll.</summary>
        public int StudentId { get; set; }
    }
}
