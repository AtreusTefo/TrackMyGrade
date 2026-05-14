using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TrackMyGradeAPI.Migrations;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Data
{
    /// <summary>EF6 database context for TrackMyGrade, including referential integrity rules.</summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>Creates a new database context using the DefaultConnection connection string.</summary>
        public ApplicationDbContext() : base("DefaultConnection") { }

        /// <summary>Teachers managed by the application.</summary>
        public DbSet<Teacher> Teachers { get; set; }

        /// <summary>Students managed by the application.</summary>
        public DbSet<Student> Students { get; set; }

        /// <summary>Courses offered by the school.</summary>
        public DbSet<Course> Courses { get; set; }

        /// <summary>Class groups taught by teachers.</summary>
        public DbSet<ClassGroup> ClassGroups { get; set; }

        /// <summary>Enrollments connecting students to class groups.</summary>
        public DbSet<StudentEnrollment> StudentEnrollments { get; set; }

        /// <summary>Assignments created for class groups.</summary>
        public DbSet<Assignment> Assignments { get; set; }

        /// <summary>Submissions for student assignments.</summary>
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }

        /// <summary>Administrative audit records.</summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>Administrators.</summary>
        public DbSet<Admin> Admins { get; set; }

        /// <summary>Configures EF6 entity relationships, indexes, and integrity constraints.</summary>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Teachers
            var teacher = modelBuilder.Entity<Teacher>();
            teacher.HasKey(e => e.Id);
            teacher.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            teacher.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            teacher.Property(e => e.Email).IsRequired().HasMaxLength(255)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Teachers_Email") { IsUnique = true }));
            teacher.Property(e => e.Phone).IsRequired().HasMaxLength(8);
            teacher.Property(e => e.Subject).IsRequired().HasMaxLength(100);
            teacher.Property(e => e.PasswordHash).IsOptional().HasMaxLength(255);
            teacher.Property(e => e.Token).IsOptional().HasMaxLength(255);
            teacher.Property(e => e.ActivationToken).IsOptional().HasMaxLength(255);

            teacher.HasMany(t => t.Students)
                .WithRequired(s => s.Teacher)
                .HasForeignKey(s => s.TeacherId)
                .WillCascadeOnDelete(false);

            teacher.HasMany(t => t.ClassGroups)
                .WithRequired(cg => cg.Teacher)
                .HasForeignKey(cg => cg.TeacherId)
                .WillCascadeOnDelete(false);

            teacher.HasMany(t => t.Assignments)
                .WithRequired(a => a.CreatedBy)
                .HasForeignKey(a => a.CreatedByTeacherId)
                .WillCascadeOnDelete(false);

            // Students
            var student = modelBuilder.Entity<Student>();
            student.HasKey(e => e.Id);
            student.Property(e => e.StudentNumber).IsRequired().HasMaxLength(20)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Students_StudentNumber") { IsUnique = true }));
            student.Property(e => e.OmangOrPassport).IsRequired().HasMaxLength(20)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Students_OmangOrPassport") { IsUnique = true }));
            student.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            student.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            student.Property(e => e.Email).IsRequired().HasMaxLength(255)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Students_Email") { IsUnique = true }));
            student.Property(e => e.Phone).IsRequired().HasMaxLength(8);
            student.Property(e => e.PasswordHash).IsOptional().HasMaxLength(255);
            student.Property(e => e.Token).IsOptional().HasMaxLength(255);
            student.Property(e => e.ActivationToken).IsOptional().HasMaxLength(255);

            student.HasRequired(e => e.Teacher)
                .WithMany(t => t.Students)
                .HasForeignKey(e => e.TeacherId)
                .WillCascadeOnDelete(false);

            student.HasMany(e => e.Enrollments)
                .WithRequired(se => se.Student)
                .HasForeignKey(se => se.StudentId)
                .WillCascadeOnDelete(true);

            student.HasMany(e => e.Submissions)
                .WithRequired(ss => ss.Student)
                .HasForeignKey(ss => ss.StudentId)
                .WillCascadeOnDelete(true);

            // Courses
            var course = modelBuilder.Entity<Course>();
            course.HasKey(e => e.Id);
            course.Property(e => e.Name).IsRequired().HasMaxLength(100);
            course.Property(e => e.Code).IsRequired().HasMaxLength(50)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Courses_Code") { IsUnique = true }));
            course.Property(e => e.Description).IsOptional().HasMaxLength(1000);
            course.Property(e => e.CreatedAt).IsRequired();
            course.Property(e => e.UpdatedAt).IsRequired();
            course.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            course.HasMany(c => c.ClassGroups)
                .WithRequired(g => g.Course)
                .HasForeignKey(g => g.CourseId)
                .WillCascadeOnDelete(false);

            // ClassGroups
            var classGroup = modelBuilder.Entity<ClassGroup>();
            classGroup.HasKey(e => e.Id);
            classGroup.Property(e => e.Name).IsRequired().HasMaxLength(100)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_ClassGroups_TeacherId_Name", 2) { IsUnique = true }));
            classGroup.Property(e => e.GradeLevel).IsRequired();
            classGroup.Property(e => e.CourseId).IsRequired()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_ClassGroups_TeacherId_Name", 1) { IsUnique = true }));
            classGroup.Property(e => e.TeacherId).IsRequired();
            classGroup.Property(e => e.CreatedAt).IsRequired();
            classGroup.Property(e => e.UpdatedAt).IsRequired();
            classGroup.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            classGroup.HasRequired(e => e.Course)
                .WithMany(c => c.ClassGroups)
                .HasForeignKey(e => e.CourseId)
                .WillCascadeOnDelete(false);

            classGroup.HasRequired(e => e.Teacher)
                .WithMany(t => t.ClassGroups)
                .HasForeignKey(e => e.TeacherId)
                .WillCascadeOnDelete(false);

            classGroup.HasMany(e => e.Enrollments)
                .WithRequired(se => se.ClassGroup)
                .HasForeignKey(se => se.ClassGroupId)
                .WillCascadeOnDelete(true);

            classGroup.HasMany(e => e.Assignments)
                .WithRequired(a => a.ClassGroup)
                .HasForeignKey(a => a.ClassGroupId)
                .WillCascadeOnDelete(true);

            // StudentEnrollments
            var enrollment = modelBuilder.Entity<StudentEnrollment>();
            enrollment.HasKey(e => e.Id);
            enrollment.Property(e => e.EnrolledAt).IsRequired();
            enrollment.Property(e => e.UpdatedAt).IsRequired();
            enrollment.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
            enrollment.Property(e => e.StudentId).HasColumnAnnotation(IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_StudentEnrollment_StudentId_ClassGroupId", 1) { IsUnique = true }));
            enrollment.Property(e => e.ClassGroupId).HasColumnAnnotation(IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_StudentEnrollment_StudentId_ClassGroupId", 2) { IsUnique = true }));

            enrollment.HasRequired(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .WillCascadeOnDelete(true);

            enrollment.HasRequired(e => e.ClassGroup)
                .WithMany(cg => cg.Enrollments)
                .HasForeignKey(e => e.ClassGroupId)
                .WillCascadeOnDelete(true);

            // Assignments
            var assignment = modelBuilder.Entity<Assignment>();
            assignment.HasKey(e => e.Id);
            assignment.Property(e => e.Title).IsRequired().HasMaxLength(200);
            assignment.Property(e => e.Description).IsOptional().HasMaxLength(2000);
            assignment.Property(e => e.DueDate).IsRequired();
            assignment.Property(e => e.MaxScore).IsRequired();
            assignment.Property(e => e.CreatedByTeacherId).IsRequired();
            assignment.Property(e => e.CreatedAt).IsRequired();
            assignment.Property(e => e.UpdatedAt).IsRequired();
            assignment.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            assignment.HasRequired(e => e.ClassGroup)
                .WithMany(cg => cg.Assignments)
                .HasForeignKey(e => e.ClassGroupId)
                .WillCascadeOnDelete(true);

            assignment.HasRequired(e => e.CreatedBy)
                .WithMany(t => t.Assignments)
                .HasForeignKey(e => e.CreatedByTeacherId)
                .WillCascadeOnDelete(false);

            assignment.HasMany(e => e.Submissions)
                .WithRequired(s => s.Assignment)
                .HasForeignKey(s => s.AssignmentId)
                .WillCascadeOnDelete(true);

            // AssignmentSubmissions
            var submission = modelBuilder.Entity<AssignmentSubmission>();
            submission.HasKey(e => e.Id);
            submission.Property(e => e.SubmittedAt).IsRequired();
            submission.Property(e => e.UpdatedAt).IsRequired();
            submission.Property(e => e.Content).IsRequired().HasMaxLength(2000);
            submission.Property(e => e.Score).IsOptional();
            submission.Property(e => e.Feedback).IsOptional().HasMaxLength(2000);
            submission.Property(e => e.Status).IsRequired().HasMaxLength(50);
            submission.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            submission.HasRequired(e => e.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(e => e.AssignmentId)
                .WillCascadeOnDelete(true);

            submission.HasRequired(e => e.Student)
                .WithMany(s => s.Submissions)
                .HasForeignKey(e => e.StudentId)
                .WillCascadeOnDelete(true);

            // AuditLogs
            var audit = modelBuilder.Entity<AuditLog>();
            audit.HasKey(e => e.Id);
            audit.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            audit.Property(e => e.Action).IsRequired().HasMaxLength(20);
            audit.Property(e => e.Changes).IsOptional().HasMaxLength(4000);
            audit.Property(e => e.PerformedBy).IsOptional().HasMaxLength(100);
            audit.Property(e => e.IpAddress).IsOptional().HasMaxLength(50);
            audit.Property(e => e.UserAgent).IsOptional().HasMaxLength(255);
            audit.Property(e => e.PerformedAt).IsRequired();

            audit.Property(e => e.EntityType)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_AuditLogs_EntityType_EntityId", 1)));
            audit.Property(e => e.EntityId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_AuditLogs_EntityType_EntityId", 2)));
            audit.Property(e => e.PerformedAt)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_AuditLogs_PerformedAt")));

            // Admins
            var admin = modelBuilder.Entity<Admin>();
            admin.HasKey(e => e.Id);
            admin.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            admin.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            admin.Property(e => e.Email).IsRequired().HasMaxLength(255)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Admins_Email") { IsUnique = true }));
            admin.Property(e => e.Phone).IsRequired().HasMaxLength(8);
            admin.Property(e => e.Password).IsRequired().HasMaxLength(255);
            admin.Property(e => e.CreatedAt).IsRequired();
            admin.Property(e => e.UpdatedAt).IsRequired();
        }

        /// <summary>
        /// Initializes the database by running all pending Code First Migrations.
        /// Uses MigrateDatabaseToLatestVersion so the EF model hash stays in sync
        /// with the schema after every model change — replacing the old
        /// CreateDatabaseIfNotExists initializer that threw on schema drift.
        /// </summary>
        public static void Initialize()
        {
            // MigrateDatabaseToLatestVersion applies any pending migrations (including
            // AddPhoneToAdmin) and keeps __MigrationHistory current, preventing the
            // "model has changed" InvalidOperationException.
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());

            using (var context = new ApplicationDbContext())
            {
                // Triggers migration run and model-hash update.
                context.Database.Initialize(true);
                EnsureSqlServerCheckConstraints(context);

                // Fallback seed: Configuration.Seed() handles this via migrations,
                // but this guard ensures the default admin exists on first run.
                if (!context.Admins.Any(a => a.Email == "admin@school.com"))
                {
                    context.Admins.Add(new Admin
                    {
                        FirstName = "System",
                        LastName = "Admin",
                        Email = "admin@school.com",
                        Phone = "71234567",
                        Password = "$2a$11$F/NmweY.Jk.ddRIkhzD4Du.pTCIHHaBDr1YArTiX4PR65ddykJ0km",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    context.SaveChanges();
                }
            }
        }

        private static void EnsureSqlServerCheckConstraints(ApplicationDbContext context)
        {
            if (context.Database.Connection.State != ConnectionState.Open)
            {
                context.Database.Connection.Open();
            }

            context.Database.ExecuteSqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Students_Phone_Format')
                   ALTER TABLE [Students] ADD CONSTRAINT [CK_Students_Phone_Format]
                   CHECK ([Phone] NOT LIKE '%[^0-9]%' AND LEN([Phone]) = 8);"
            );

            context.Database.ExecuteSqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Teachers_Phone_Format')
                   ALTER TABLE [Teachers] ADD CONSTRAINT [CK_Teachers_Phone_Format]
                   CHECK ([Phone] NOT LIKE '%[^0-9]%' AND LEN([Phone]) = 8);"
            );

            context.Database.ExecuteSqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Admins_Phone_Format')
                   ALTER TABLE [Admins] ADD CONSTRAINT [CK_Admins_Phone_Format]
                   CHECK ([Phone] NOT LIKE '%[^0-9]%' AND LEN([Phone]) = 8);"
            );

            context.Database.ExecuteSqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Students_Email_Lowercase')
                   ALTER TABLE [Students] ADD CONSTRAINT [CK_Students_Email_Lowercase]
                   CHECK ([Email] = LOWER([Email]));"
            );

            context.Database.ExecuteSqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Teachers_Email_Lowercase')
                   ALTER TABLE [Teachers] ADD CONSTRAINT [CK_Teachers_Email_Lowercase]
                   CHECK ([Email] = LOWER([Email]));"
            );

            context.Database.ExecuteSqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Students_Grade_Range')
                   ALTER TABLE [Students] ADD CONSTRAINT [CK_Students_Grade_Range]
                   CHECK ([Grade] BETWEEN 7 AND 12);"
            );

            context.Database.ExecuteSqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_AssignmentSubmissions_Score_Positive')
                   ALTER TABLE [AssignmentSubmissions] ADD CONSTRAINT [CK_AssignmentSubmissions_Score_Positive]
                   CHECK ([Score] IS NULL OR [Score] >= 0);"
            );
        }
    }
}
