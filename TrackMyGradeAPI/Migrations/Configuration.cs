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
        /// Seeds initial data for the application.
        /// </summary>
        /// <param name="context">The database context to seed.</param>
        protected override void Seed(ApplicationDbContext context)
        {
            base.Seed(context);

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
    }
}