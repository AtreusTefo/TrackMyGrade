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
            // EF6 creates the LocalDB database and schema automatically if they don't exist
            Database.SetInitializer(new CreateDatabaseIfNotExists<ApplicationDbContext>());
            using (var context = new ApplicationDbContext())
            {
                context.Database.Initialize(false);
            }
        }
    }
}
