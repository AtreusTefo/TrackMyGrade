using System;
using System.Data.Entity.Migrations;
using System.Linq;
using BCrypt.Net;
using TrackMyGradeAPI.Models;
using TrackMyGradeAPI.Data;

namespace TrackMyGradeAPI.Migrations
{
    /// <summary>Entity Framework 6 Code First Migrations configuration for TrackMyGrade.</summary>
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        /// <summary>Creates a new migrations configuration.</summary>
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsNamespace = "TrackMyGradeAPI.Migrations";
            MigrationsDirectory = "Migrations";
        }

        /// <summary>
        /// Runs when the database is created or migrated.
        /// Seeds initial data for the application and ensures data integrity.
        /// </summary>
        /// <param name="context">The database context to seed.</param>
        protected override void Seed(ApplicationDbContext context)
        {
            base.Seed(context);

            // Ensure data integrity constraints exist
            EnsureDataIntegrityConstraints(context);

            // Add default admin account if not present
            if (!context.Admins.Any(a => a.Email == "admin@school.com"))
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
                string normalizedEmail = "admin@school.com".ToLower();

                context.Admins.Add(new Admin
                {
                    FirstName = "System",
                    LastName = "Admin",
                    Email = normalizedEmail,
                    Phone = "71234567",
                    Password = passwordHash,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"[Seeding] Default admin account created: {normalizedEmail}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[Seeding] Default admin account already exists, skipping seed.");
            }
        }

        /// <summary>Ensures all data integrity constraints are in place in the database.</summary>
        private void EnsureDataIntegrityConstraints(ApplicationDbContext context)
        {
            try
            {
                var connection = context.Database.Connection;
                connection.Open();
                var command = connection.CreateCommand();

                // Drop existing constraints if they exist (idempotent)
                var dropConstraintsSql = @"
                    -- Drop check constraints if they exist
                    IF OBJECT_ID('CK_Students_Phone_Format', 'C') IS NOT NULL
                        ALTER TABLE [Students] DROP CONSTRAINT [CK_Students_Phone_Format];
                    IF OBJECT_ID('CK_Teachers_Phone_Format', 'C') IS NOT NULL
                        ALTER TABLE [Teachers] DROP CONSTRAINT [CK_Teachers_Phone_Format];
                    IF OBJECT_ID('CK_Students_Email_Lowercase', 'C') IS NOT NULL
                        ALTER TABLE [Students] DROP CONSTRAINT [CK_Students_Email_Lowercase];
                    IF OBJECT_ID('CK_Teachers_Email_Lowercase', 'C') IS NOT NULL
                        ALTER TABLE [Teachers] DROP CONSTRAINT [CK_Teachers_Email_Lowercase];
                    IF OBJECT_ID('CK_Students_Grade_Range', 'C') IS NOT NULL
                        ALTER TABLE [Students] DROP CONSTRAINT [CK_Students_Grade_Range];
                    IF OBJECT_ID('CK_AssignmentSubmissions_Score_Positive', 'C') IS NOT NULL
                        ALTER TABLE [AssignmentSubmissions] DROP CONSTRAINT [CK_AssignmentSubmissions_Score_Positive];
                    IF OBJECT_ID('CK_Assignments_MaxScore_Positive', 'C') IS NOT NULL
                        ALTER TABLE [Assignments] DROP CONSTRAINT [CK_Assignments_MaxScore_Positive];
                    IF OBJECT_ID('CK_AssignmentSubmissions_Score_Within_MaxScore', 'C') IS NOT NULL
                        ALTER TABLE [AssignmentSubmissions] DROP CONSTRAINT [CK_AssignmentSubmissions_Score_Within_MaxScore];
                ";

                command.CommandText = dropConstraintsSql;
                command.ExecuteNonQuery();

                // Add data integrity check constraints
                var addConstraintsSql = @"
                    -- Phone format validation (8 digits)
                    ALTER TABLE [Students] ADD CONSTRAINT [CK_Students_Phone_Format]
                    CHECK ([Phone] NOT LIKE '%[^0-9]%' AND LEN([Phone]) = 8);

                    ALTER TABLE [Teachers] ADD CONSTRAINT [CK_Teachers_Phone_Format]
                    CHECK ([Phone] NOT LIKE '%[^0-9]%' AND LEN([Phone]) = 8);

                    -- Email lowercase enforcement
                    ALTER TABLE [Students] ADD CONSTRAINT [CK_Students_Email_Lowercase]
                    CHECK ([Email] = LOWER([Email]));

                    ALTER TABLE [Teachers] ADD CONSTRAINT [CK_Teachers_Email_Lowercase]
                    CHECK ([Email] = LOWER([Email]));

                    -- Student grade range (7-12)
                    ALTER TABLE [Students] ADD CONSTRAINT [CK_Students_Grade_Range]
                    CHECK ([Grade] BETWEEN 7 AND 12);

                    -- Assignment submission score validation
                    ALTER TABLE [AssignmentSubmissions] ADD CONSTRAINT [CK_AssignmentSubmissions_Score_Positive]
                    CHECK ([Score] IS NULL OR [Score] >= 0);

                    -- Assignment max score must be positive
                    ALTER TABLE [Assignments] ADD CONSTRAINT [CK_Assignments_MaxScore_Positive]
                    CHECK ([MaxScore] > 0);
                ";

                command.CommandText = addConstraintsSql;
                command.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception ex)
            {
                // Log constraint creation errors but don't fail the migration
                System.Diagnostics.Debug.WriteLine($"Warning: Could not ensure data integrity constraints: {ex.Message}");
            }
        }
    }
}
