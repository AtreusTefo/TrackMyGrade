using System;

namespace TrackMyGradeAPI.DTOs
{
    // ── Admin auth ────────────────────────────────────────────────────────

    /// <summary>Admin login request.</summary>
    public class AdminLoginDto
    {
        public string Email    { get; set; }
        public string Password { get; set; }
    }

    /// <summary>Admin login response — JWT only (no personal profile).</summary>
    public class AdminResponseDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }

    // ── Admin → Teacher management ────────────────────────────────────────

    /// <summary>Request to create a new teacher account (admin only). No password — activation flow sets it.</summary>
    public class AdminCreateTeacherDto
    {
        public string FirstName { get; set; }
        public string LastName  { get; set; }
        public string Email     { get; set; }
        public string Phone     { get; set; }
        public string Subject   { get; set; }
    }

    /// <summary>Teacher record as seen by the admin dashboard.</summary>
    public class AdminTeacherDto
    {
        public int       Id              { get; set; }
        public string    FirstName       { get; set; }
        public string    LastName        { get; set; }
        public string    Email           { get; set; }
        public string    Phone           { get; set; }
        public string    Subject         { get; set; }
        public bool      IsActivated     { get; set; }
        /// <summary>Returned only for unactivated accounts so admin can share the link.</summary>
        public string    ActivationToken { get; set; }
        public DateTime? ActivatedAt     { get; set; }
    }

    // ── Admin → Student management ────────────────────────────────────────

    /// <summary>Request to create a new student (admin only). No password — activation flow sets it.</summary>
    public class AdminCreateStudentDto
    {
        public string FirstName       { get; set; }
        public string LastName        { get; set; }
        public string Email           { get; set; }
        public string Phone           { get; set; }
        public string OmangOrPassport { get; set; }
        public int    Grade           { get; set; }
        public int    TeacherId       { get; set; }   // primary teacher (for legacy student list)
    }

    /// <summary>Request to update a student's personal details (admin only).</summary>
    public class AdminUpdateStudentDto
    {
        public string FirstName       { get; set; }
        public string LastName        { get; set; }
        public string Email           { get; set; }
        public string Phone           { get; set; }
        public string OmangOrPassport { get; set; }
        public int    Grade           { get; set; }
        public int    TeacherId       { get; set; }
    }

    /// <summary>Student record as seen by the admin dashboard.</summary>
    public class AdminStudentDto
    {
        public int       Id              { get; set; }
        public string    StudentNumber   { get; set; }
        public string    FirstName       { get; set; }
        public string    LastName        { get; set; }
        public string    Email           { get; set; }
        public string    Phone           { get; set; }
        public string    OmangOrPassport { get; set; }
        public int       Grade           { get; set; }
        public int       TeacherId       { get; set; }
        public bool      IsActivated     { get; set; }
        /// <summary>Returned only for unactivated accounts.</summary>
        public string    ActivationToken { get; set; }
        public DateTime? ActivatedAt     { get; set; }
    }

    // ── Admin → Course / ClassGroup ───────────────────────────────────────

    public class CreateCourseDto
    {
        public string Name        { get; set; }
        public string Code        { get; set; }
        public string Description { get; set; }
    }

    public class CourseDto
    {
        public int    Id          { get; set; }
        public string Name        { get; set; }
        public string Code        { get; set; }
        public string Description { get; set; }
    }

    public class CreateClassGroupDto
    {
        public string Name       { get; set; }
        public int    GradeLevel { get; set; }
        public int    CourseId   { get; set; }
        public int    TeacherId  { get; set; }
    }

    public class ClassGroupDto
    {
        public int    Id          { get; set; }
        public string Name        { get; set; }
        public int    GradeLevel  { get; set; }
        public int    CourseId    { get; set; }
        public string CourseName  { get; set; }
        public int    TeacherId   { get; set; }
        public string TeacherName { get; set; }
    }

    public class EnrollStudentDto
    {
        public int StudentId { get; set; }
    }
}
