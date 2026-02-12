namespace TrackMyGradeAPI.DTOs
{
    public class TeacherRegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; }
        public string Password { get; set; }
    }

    public class TeacherLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class TeacherResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; }
        public string Token { get; set; }
    }

    public class StudentCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Grade { get; set; }
        public int Assessment1 { get; set; }
        public int Assessment2 { get; set; }
        public int Assessment3 { get; set; }
    }

    public class StudentUpdateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Grade { get; set; }
        public int Assessment1 { get; set; }
        public int Assessment2 { get; set; }
        public int Assessment3 { get; set; }
    }

    public class StudentResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Grade { get; set; }
        public int Assessment1 { get; set; }
        public int Assessment2 { get; set; }
        public int Assessment3 { get; set; }
        public int Total { get; set; }
        public double Average { get; set; }
        public double Percentage { get; set; }
        public string PerformanceLevel { get; set; }
    }
}
