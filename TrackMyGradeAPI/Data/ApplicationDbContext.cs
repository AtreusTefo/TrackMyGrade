using System.Data.Entity;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Teacher>().HasKey(t => t.Id);
            modelBuilder.Entity<Teacher>()
                .Property(t => t.Email).IsRequired().HasMaxLength(255);

            modelBuilder.Entity<Student>().HasKey(s => s.Id);
            modelBuilder.Entity<Student>()
                .Property(s => s.StudentNumber).IsRequired().HasMaxLength(20);
            modelBuilder.Entity<Student>()
                .Property(s => s.OmangOrPassport).IsRequired().HasMaxLength(9);
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
            // Disable EF's built-in model-compatibility check entirely.
            // DropCreateDatabaseIfModelChanges proved unreliable when the DB was
            // originally created by CreateDatabaseIfNotExists (open VS connections
            // and stale __MigrationHistory hashes silently block the drop).
            // Instead we manage schema evolution explicitly with conditional SQL so
            // existing data is preserved and new columns are added automatically.
            Database.SetInitializer<ApplicationDbContext>(null);

            using (var context = new ApplicationDbContext())
            {
                // Creates the full schema from the current EF model on a fresh install;
                // returns false without touching anything when the DB already exists.
                context.Database.CreateIfNotExists();

                // Add StudentNumber if the column is missing (pre-dates this column).
                // The EXEC wrapper compiles the backfill UPDATE in a child scope that
                // runs after ALTER TABLE has committed the column, avoiding the SQL
                // Server parse-time error "Invalid column name 'StudentNumber'" that
                // occurs when both statements share the same T-SQL batch.
                context.Database.ExecuteSqlCommand(
                    @"IF NOT EXISTS (
                          SELECT 1 FROM sys.columns
                          WHERE  object_id = OBJECT_ID(N'dbo.Students')
                          AND    name      = N'StudentNumber')
                      BEGIN
                          ALTER TABLE dbo.Students
                              ADD StudentNumber NVARCHAR(20) NOT NULL DEFAULT '';
                          EXEC(
                              'UPDATE dbo.Students
                               SET    StudentNumber =
                                          ''STU-'' + CAST(YEAR(GETDATE()) AS NVARCHAR(4))
                                          + ''-'' + RIGHT(''0000'' + CAST(Id AS NVARCHAR(10)), 4)
                               WHERE  StudentNumber = ''''')
                      END");

                // Add OmangOrPassport if the column is missing (pre-dates this column).
                context.Database.ExecuteSqlCommand(
                    @"IF NOT EXISTS (
                          SELECT 1 FROM sys.columns
                          WHERE  object_id = OBJECT_ID(N'dbo.Students')
                          AND    name      = N'OmangOrPassport')
                      ALTER TABLE dbo.Students
                          ADD OmangOrPassport NVARCHAR(9) NOT NULL DEFAULT ''");
            }
        }
    }
}
