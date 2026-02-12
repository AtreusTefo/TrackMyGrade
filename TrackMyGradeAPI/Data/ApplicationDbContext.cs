using System.Data.Entity;
using TrackMyGradeAPI.Models;

namespace TrackMyGradeAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
            // Configure for in-memory database
        }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Teacher configuration
            modelBuilder.Entity<Teacher>()
                .HasKey(t => t.Id);
            
            modelBuilder.Entity<Teacher>()
                .Property(t => t.Email)
                .IsRequired()
                .HasMaxLength(255);

            // Student configuration
            modelBuilder.Entity<Student>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Student>()
                .HasRequired(s => s.Teacher)
                .WithMany()
                .HasForeignKey(s => s.TeacherId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Student>()
                .Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(255);

            // Seed initial data
            //modelBuilder.Entity<Teacher>().HasData(
            //    new Teacher { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", 
            //        Phone = "12345678", Subject = "Math", Password = "password123" }
            //);
        }

        public static void Initialize()
        {
            using (var context = new ApplicationDbContext())
            {
                context.Database.CreateIfNotExists();
            }
        }
    }
}
