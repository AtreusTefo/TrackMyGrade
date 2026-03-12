namespace TrackMyGradeAPI.DTOs
{
    /// <summary>Request body for registering a new teacher.</summary>
    public class TeacherRegisterDto
    {
        /// <summary>Teacher's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Teacher's last name.</summary>
        public string LastName { get; set; }
        /// <summary>Unique email address used for login.</summary>
        public string Email { get; set; }
        /// <summary>Contact phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Subject the teacher instructs (e.g. Mathematics).</summary>
        public string Subject { get; set; }
        /// <summary>Password (minimum 6 characters).</summary>
        public string Password { get; set; }
    }

    /// <summary>Request body for teacher login.</summary>
    public class TeacherLoginDto
    {
        /// <summary>Registered email address.</summary>
        public string Email { get; set; }
        /// <summary>Account password.</summary>
        public string Password { get; set; }
    }

    /// <summary>Teacher profile returned after register or login.</summary>
    public class TeacherResponseDto
    {
        /// <summary>Unique teacher identifier.</summary>
        public int Id { get; set; }
        /// <summary>Teacher's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Teacher's last name.</summary>
        public string LastName { get; set; }
        /// <summary>Teacher's email address.</summary>
        public string Email { get; set; }
        /// <summary>Teacher's phone number.</summary>
        public string Phone { get; set; }
        /// <summary>Subject the teacher instructs.</summary>
        public string Subject { get; set; }
        /// <summary>Auth token to be sent as the X-TeacherId header value on subsequent requests.</summary>
        public string Token { get; set; }
    }

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
        /// <summary>Academic grade / year (e.g. 10).</summary>
        public int Grade { get; set; }
        /// <summary>Score for Assessment 1 (0–20).</summary>
        public int Assessment1 { get; set; }
        /// <summary>Score for Assessment 2 (0–20).</summary>
        public int Assessment2 { get; set; }
        /// <summary>Score for Assessment 3 (0–20).</summary>
        public int Assessment3 { get; set; }
    }

    /// <summary>Request body for creating a new student.</summary>
    public class StudentCreateDto : StudentDtoBase { }

    /// <summary>Request body for updating an existing student.</summary>
    public class StudentUpdateDto : StudentDtoBase { }

    /// <summary>Full student record including computed performance metrics.</summary>
    public class StudentResponseDto
    {
        /// <summary>Unique student identifier.</summary>
        public int Id { get; set; }
        /// <summary>Student's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>Student's last name.</summary>
        public string LastName { get; set; }
        /// <summary>Student's email address.</summary>
        public string Email { get; set; }
        /// <summary>Student's phone number.</summary>
        public string Phone { get; set; }
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
}
