namespace TrackMyGradeAPI.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; }
        public string Password { get; set; } // Plain text for now
        public string Token { get; set; } // Simple token
    }

    public class Student
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Grade { get; set; }
        public int Assessment1 { get; set; } // 0-20
        public int Assessment2 { get; set; } // 0-20
        public int Assessment3 { get; set; } // 0-20

        // Navigation property
        public Teacher Teacher { get; set; }

        // Calculated properties
        public int Total => Assessment1 + Assessment2 + Assessment3;
        public double Average => Total / 3.0;
        public double Percentage => (Total / 60.0) * 100;
        public string PerformanceLevel
        {
            get
            {
                if (Percentage < 50) return "Needs Support";
                if (Percentage <= 55) return "Satisfactory";
                if (Percentage <= 75) return "Good";
                return "Excellent";
            }
        }
    }
}
