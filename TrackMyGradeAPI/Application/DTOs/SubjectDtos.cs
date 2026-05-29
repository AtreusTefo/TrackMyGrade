namespace TrackMyGradeAPI.DTOs
{
    /// <summary>Details of a school subject.</summary>
    public class SubjectDto
    {
        /// <summary>Gets or sets the subject ID.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the full name of the subject.</summary>
        public string Name { get; set; }
        /// <summary>Gets or sets the unique subject code.</summary>
        public string Code { get; set; }
        /// <summary>Gets or sets the optional subject description.</summary>
        public string Description { get; set; }
    }

    /// <summary>Data required to create a new subject.</summary>
    public class CreateSubjectDto
    {
        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }
        /// <summary>Gets or sets the code.</summary>
        public string Code { get; set; }
        /// <summary>Gets or sets the description.</summary>
        public string Description { get; set; }
    }

    /// <summary>Details of a class group including assigned subject and teacher.</summary>
    public class ClassGroupDto
    {
        /// <summary>Gets or sets the class group ID.</summary>
        public int Id { get; set; }
        /// <summary>Gets or sets the group name.</summary>
        public string Name { get; set; }
        /// <summary>Gets or sets the academic grade level.</summary>
        public int GradeLevel { get; set; }
        /// <summary>Gets or sets the subject ID.</summary>
        public int SubjectId { get; set; }
        /// <summary>Gets or sets the subject name.</summary>
        public string SubjectName { get; set; }
        /// <summary>Gets or sets the teacher ID.</summary>
        public int TeacherId { get; set; }
        /// <summary>Gets or sets the full name of the teacher.</summary>
        public string TeacherName { get; set; }
    }

    /// <summary>Data required to create a new class group.</summary>
    public class CreateClassGroupDto
    {
        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }
        /// <summary>Gets or sets the grade level.</summary>
        public int GradeLevel { get; set; }
        /// <summary>Gets or sets the associated subject ID.</summary>
        public int SubjectId { get; set; }
        /// <summary>Gets or sets the associated teacher ID.</summary>
        public int TeacherId { get; set; }
    }

    /// <summary>Request body for enrolling a student in a class group.</summary>
    public class EnrollStudentDto
    {
        /// <summary>Gets or sets the student ID to enroll.</summary>
        public int StudentId { get; set; }
    }
}