namespace TrackMyGradeAPI.DTOs
{
    /// <summary>Shared student properties used by both create and update requests.</summary>
    public abstract class StudentDtoBase
    {
        /// <summary>Student's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Student's last name.</summary>
        public string LastName { get; set; }
        /// <summary>Student's email address.</summary>
        public string Email { get; set; }
        /// <summary>Student's phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Student's Omang number (9-digit national ID) or 9-character passport number.</summary>
        public string OmangOrPassport { get; set; }
        /// <summary>Academic grade / year (e.g. 10).</summary>
        public int Grade { get; set; }
        /// <summary>Score for Assessment 1 (0–20).</summary>
        public int Assessment1 { get; set; }
        /// <summary>Score for Assessment 2 (0–20).</summary>
        public int Assessment2 { get; set; }
        /// <summary>Score for Assessment 3 (0–20).</summary>
        public int Assessment3 { get; set; }
        /// <summary>Login password for the student. Required on create, optional on update.</summary>
        public string Password { get; set; }
    }

    /// <summary>Request body for creating a new student.</summary>
    public class StudentCreateDto : StudentDtoBase { }

    /// <summary>Request body for updating an existing student.</summary>
    public class StudentUpdateDto : StudentDtoBase { }

    /// <summary>Full student record including computed performance metrics.</summary>
    public class StudentResponseDto
    {
        /// <summary>Database primary key.</summary>
        public int Id { get; set; }
        /// <summary>System-generated unique student number (e.g. STU-2026-0001).</summary>
        public string StudentNumber { get; set; }
        /// <summary>Student's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Student's last name.</summary>
        public string LastName { get; set; }
        /// <summary>Student's email address.</summary>
        public string Email { get; set; }
        /// <summary>Student's phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Student's Omang number or passport number.</summary>
        public string OmangOrPassport { get; set; }
        /// <summary>Academic grade / year.</summary>
        public int Grade { get; set; }
        /// <summary>Score for Assessment 1 (0–20).</summary>
        public int Assessment1 { get; set; }
        /// <summary>Score for Assessment 2 (0–20).</summary>
        public int Assessment2 { get; set; }
        /// <summary>Score for Assessment 3 (0–20).</summary>
        public int Assessment3 { get; set; }
        /// <summary>Sum of all three assessment scores (max 60).</summary>
        public int Total { get; set; }
        /// <summary>Mean score across all three assessments.</summary>
        public double Average { get; set; }
        /// <summary>Total expressed as a percentage of the maximum possible score (60).</summary>
        public double Percentage { get; set; }
        /// <summary>Performance band: Excellent, Good, Satisfactory, or Needs Support.</summary>
        public string PerformanceLevel { get; set; }
    }

    // ── Student login DTOs ──────────────────────────────────────────────

    /// <summary>Request body for student login.</summary>
    public class StudentLoginDto
    {
        /// <summary>Registered email address.</summary>
        public string Email { get; set; }
        /// <summary>Account password.</summary>
        public string Password { get; set; }
    }

    /// <summary>Student profile returned after register or login.</summary>
    public class StudentAuthResponseDto
    {
        /// <summary>Database primary key.</summary>
        public int Id { get; set; }
        /// <summary>System-generated unique student number.</summary>
        public string StudentNumber { get; set; }
        /// <summary>Student's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Student's last name.</summary>
        public string LastName { get; set; }
        /// <summary>Student's email address.</summary>
        public string Email { get; set; }
        /// <summary>Student's phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Student's Omang number or passport number.</summary>
        public string OmangOrPassport { get; set; }
        /// <summary>Academic grade / year.</summary>
        public int Grade { get; set; }
        /// <summary>Score for Assessment 1 (0–20).</summary>
        public int Assessment1 { get; set; }
        /// <summary>Score for Assessment 2 (0–20).</summary>
        public int Assessment2 { get; set; }
        /// <summary>Score for Assessment 3 (0–20).</summary>
        public int Assessment3 { get; set; }
        /// <summary>Sum of all three assessment scores (max 60).</summary>
        public int Total { get; set; }
        /// <summary>Mean score across all three assessments.</summary>
        public double Average { get; set; }
        /// <summary>Total expressed as a percentage of the maximum possible score (60).</summary>
        public double Percentage { get; set; }
        /// <summary>Performance band: Excellent, Good, Satisfactory, or Needs Support.</summary>
        public string PerformanceLevel { get; set; }
        /// <summary>Auth token for subsequent requests.</summary>
        public string Token { get; set; }
    }

    /// <summary>Request body for a student submitting their own assessment scores.</summary>
    public class StudentSubmitAssessmentsDto
    {
        /// <summary>Score for Assessment 1 (0–20).</summary>
        public int Assessment1 { get; set; }
        /// <summary>Score for Assessment 2 (0–20).</summary>
        public int Assessment2 { get; set; }
        /// <summary>Score for Assessment 3 (0–20).</summary>
        public int Assessment3 { get; set; }
    }
}
