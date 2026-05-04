namespace TrackMyGradeAPI.DTOs
{
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
        
        // Computed metrics can be added later if needed based on Submissions
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

    /// <summary>Student profile returned after login.</summary>
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
        /// <summary>Auth token for subsequent requests.</summary>
        public string Token { get; set; }
    }
}
