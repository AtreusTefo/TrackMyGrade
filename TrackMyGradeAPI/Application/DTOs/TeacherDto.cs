namespace TrackMyGrade.Application.DTOs
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
}