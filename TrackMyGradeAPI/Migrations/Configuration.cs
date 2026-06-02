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
            // Enabled to allow EF to generate schema automatically from the model.
            // After automatic migrations create tables, explicit migrations like 
            // FixSubjectsConstraintName will execute to apply specific fixes.
            AutomaticMigrationsEnabled = true;

            // Set to false to prevent accidental data loss during schema updates.
            AutomaticMigrationDataLossAllowed = false;

            MigrationsNamespace = "TrackMyGradeAPI.Migrations";
            MigrationsDirectory = "Migrations";
        }

        /// <summary>
        /// Runs when the database is created or migrated.
        /// Seeds initial data for the application and ensures data integrity constraints.
        /// </summary>
        /// <param name="context">The database context to seed.</param>
        protected override void Seed(ApplicationDbContext context)
        {
            base.Seed(context);

            try
            {
                // Ensure all data integrity constraints are properly created
                // Wrapped in try-catch because constraint creation may fail if database
                // has legacy/malformed constraints from previous migrations.
                // In such cases, migrations will fix them, so constraint creation is optional.
                EnsureDataIntegrityConstraints(context);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Seeding] Non-fatal warning: Constraint seeding failed: {ex.Message}");
                // Continue seeding even if constraint creation fails
            }

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

        /// <summary>
        /// Ensures all data integrity constraints are created with proper SQL Server syntax.
        /// NOTE: Currently disabled due to schema inconsistencies in existing databases.
        /// Constraint management is now handled exclusively by migrations in FixSubjectsConstraintName
        /// and subsequent migration files. This method is kept for backward compatibility.
        /// </summary>
        /// <param name="context">The database context for executing SQL commands.</param>
        private static void EnsureDataIntegrityConstraints(ApplicationDbContext context)
        {
            // All constraint management is now handled by migrations.
            // This method left as no-op for backward compatibility and future constraint management.
            System.Diagnostics.Debug.WriteLine("[Seeding] Constraint management delegated to migrations.");
        }
    }
}