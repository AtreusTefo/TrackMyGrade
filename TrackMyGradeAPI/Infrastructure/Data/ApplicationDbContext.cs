using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using TrackMyGradeAPI.Models;
namespace TrackMyGradeAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection") { }

        // ── Existing DbSets ────────────────────────────────────────────────
        public DbSet<Teacher>    Teachers { get; set; }
        public DbSet<Student>    Students { get; set; }

        // ── New domain DbSets ──────────────────────────────────────────────
        public DbSet<Course>               Courses              { get; set; }
        public DbSet<ClassGroup>           ClassGroups          { get; set; }
        public DbSet<StudentEnrollment>    StudentEnrollments   { get; set; }
        public DbSet<Assignment>           Assignments          { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Teacher ────────────────────────────────────────────────────
            modelBuilder.Entity<Teacher>().HasKey(t => t.Id);
            modelBuilder.Entity<Teacher>()
                .Property(t => t.Email).IsRequired().HasMaxLength(255);
            modelBuilder.Entity<Teacher>()
                .Property(t => t.PasswordHash).IsOptional().HasMaxLength(255);

            // ── Student ────────────────────────────────────────────────────
            modelBuilder.Entity<Student>().HasKey(s => s.Id);
            modelBuilder.Entity<Student>()
                .Property(s => s.StudentNumber).IsRequired().HasMaxLength(20);
            modelBuilder.Entity<Student>()
                .Property(s => s.OmangOrPassport).IsRequired().HasMaxLength(9);
            modelBuilder.Entity<Student>()
                .Property(s => s.Email).IsRequired().HasMaxLength(255);
            modelBuilder.Entity<Student>()
                .Property(s => s.PasswordHash).IsOptional().HasMaxLength(255);

            // Student → primary Teacher FK (legacy, kept for backward compat reads)
            modelBuilder.Entity<Student>()
                .HasRequired(s => s.Teacher)
                .WithMany(t => t.Students)
                .HasForeignKey(s => s.TeacherId)
                .WillCascadeOnDelete(false);   // cascade handled via ClassGroup/Enrollment

            // ── Course ─────────────────────────────────────────────────────
            modelBuilder.Entity<Course>().HasKey(c => c.Id);
            modelBuilder.Entity<Course>()
                .Property(c => c.Name).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Course>()
                .Property(c => c.Code).IsRequired().HasMaxLength(20);

            // ── ClassGroup ─────────────────────────────────────────────────
            modelBuilder.Entity<ClassGroup>().HasKey(cg => cg.Id);
            modelBuilder.Entity<ClassGroup>()
                .Property(cg => cg.Name).IsRequired().HasMaxLength(100);

            modelBuilder.Entity<ClassGroup>()
                .HasRequired(cg => cg.Course)
                .WithMany(c => c.ClassGroups)
                .HasForeignKey(cg => cg.CourseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClassGroup>()
                .HasRequired(cg => cg.Teacher)
                .WithMany(t => t.ClassGroups)
                .HasForeignKey(cg => cg.TeacherId)
                .WillCascadeOnDelete(false);

            // ── StudentEnrollment ──────────────────────────────────────────
            modelBuilder.Entity<StudentEnrollment>().HasKey(e => e.Id);

            modelBuilder.Entity<StudentEnrollment>()
                .HasRequired(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .WillCascadeOnDelete(true);   // deleting a student removes enrollments

            modelBuilder.Entity<StudentEnrollment>()
                .HasRequired(e => e.ClassGroup)
                .WithMany(cg => cg.Enrollments)
                .HasForeignKey(e => e.ClassGroupId)
                .WillCascadeOnDelete(false);

            // Unique index: a student can only be enrolled once per class
            modelBuilder.Entity<StudentEnrollment>()
                .HasIndex(e => new { e.StudentId, e.ClassGroupId })
                .IsUnique();

            // ── Assignment ─────────────────────────────────────────────────
            modelBuilder.Entity<Assignment>().HasKey(a => a.Id);
            modelBuilder.Entity<Assignment>()
                .Property(a => a.Title).IsRequired().HasMaxLength(200);

            modelBuilder.Entity<Assignment>()
                .HasRequired(a => a.ClassGroup)
                .WithMany(cg => cg.Assignments)
                .HasForeignKey(a => a.ClassGroupId)
                .WillCascadeOnDelete(true);   // deleting a class removes its assignments

            modelBuilder.Entity<Assignment>()
                .HasRequired(a => a.CreatedBy)
                .WithMany(t => t.Assignments)
                .HasForeignKey(a => a.CreatedByTeacherId)
                .WillCascadeOnDelete(false);

            // ── AssignmentSubmission ───────────────────────────────────────
            modelBuilder.Entity<AssignmentSubmission>().HasKey(s => s.Id);
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.Status).IsRequired().HasMaxLength(20);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasRequired(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .WillCascadeOnDelete(true);   // deleting an assignment removes submissions

            modelBuilder.Entity<AssignmentSubmission>()
                .HasRequired(s => s.Student)
                .WithMany(st => st.Submissions)
                .HasForeignKey(s => s.StudentId)
                .WillCascadeOnDelete(false);

            // One submission per student per assignment
            modelBuilder.Entity<AssignmentSubmission>()
                .HasIndex(s => new { s.AssignmentId, s.StudentId })
                .IsUnique();
        }

        public static void Initialize()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<ApplicationDbContext>());

            using (var ctx = new ApplicationDbContext())
            {
                ctx.Database.Initialize(force: true);
                
                // Add a default admin teacher
                if (!ctx.Teachers.Any(t => t.Email == "admin@trackmygrade.com"))
                {
                    ctx.Teachers.Add(new Teacher
                    {
                        FirstName = "Admin",
                        LastName = "User",
                        Email = "admin@trackmygrade.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        Subject = "Administration",
                        IsActivated = true
                    });
                    ctx.SaveChanges();
                }
            }
        }
    }
}
