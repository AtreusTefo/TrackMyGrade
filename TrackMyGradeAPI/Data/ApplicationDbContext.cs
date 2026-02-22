using System.Data.Entity;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Data
{
    [DbConfigurationType(typeof(SQLiteConfiguration))]
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Disable the default initializer — SQLite does not support EF's SQL Server DDL
            Database.SetInitializer<ApplicationDbContext>(null);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Teacher>().HasKey(t => t.Id);
            modelBuilder.Entity<Teacher>()
                .Property(t => t.Email).IsRequired().HasMaxLength(255);

            modelBuilder.Entity<Student>().HasKey(s => s.Id);
            modelBuilder.Entity<Student>()
                .HasRequired(s => s.Teacher)
                .WithMany()
                .HasForeignKey(s => s.TeacherId)
                .WillCascadeOnDelete(true);
            modelBuilder.Entity<Student>()
                .Property(s => s.Email).IsRequired().HasMaxLength(255);

            // Ignore computed read-only properties — calculated at runtime, not stored in DB
            modelBuilder.Entity<Student>().Ignore(s => s.Total);
            modelBuilder.Entity<Student>().Ignore(s => s.Average);
            modelBuilder.Entity<Student>().Ignore(s => s.Percentage);
            modelBuilder.Entity<Student>().Ignore(s => s.PerformanceLevel);
        }

        public static void Initialize()
        {
            Database.SetInitializer<ApplicationDbContext>(null);

            using (var context = new ApplicationDbContext())
            {
                // Create tables using SQLite-compatible DDL
                context.Database.ExecuteSqlCommand(@"
                    CREATE TABLE IF NOT EXISTS Teachers (
                        Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                        FirstName TEXT,
                        LastName  TEXT,
                        Email     TEXT    NOT NULL,
                        Phone     TEXT,
                        Subject   TEXT,
                        Password  TEXT,
                        Token     TEXT
                    );
                ");

                context.Database.ExecuteSqlCommand(@"
                    CREATE TABLE IF NOT EXISTS Students (
                        Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        TeacherId   INTEGER NOT NULL,
                        FirstName   TEXT,
                        LastName    TEXT,
                        Email       TEXT    NOT NULL,
                        Phone       TEXT,
                        Grade       INTEGER NOT NULL DEFAULT 0,
                        Assessment1 INTEGER NOT NULL DEFAULT 0,
                        Assessment2 INTEGER NOT NULL DEFAULT 0,
                        Assessment3 INTEGER NOT NULL DEFAULT 0,
                        FOREIGN KEY (TeacherId) REFERENCES Teachers(Id) ON DELETE CASCADE
                    );
                ");
            }
        }
    }
}
