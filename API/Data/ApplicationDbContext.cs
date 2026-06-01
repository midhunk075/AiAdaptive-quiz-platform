using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Syllabus> Syllabi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the One-to-One relationship
            modelBuilder.Entity<Subject>()
                .HasOne(s => s.Syllabus)        // Subject has one Syllabus
                .WithOne(sy => sy.Subject)      // Syllabus has one Subject
                .HasForeignKey<Syllabus>(sy => sy.SubjectId) // The FK is in the Syllabus table
                .OnDelete(DeleteBehavior.Cascade); // If Subject is deleted, delete Syllabus too

            // Also configure the One-to-Many for Questions just to be safe
            modelBuilder.Entity<Subject>()
                .HasMany(s => s.Questions)
                .WithOne(q => q.Subject)
                .HasForeignKey(q => q.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Topic)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TopicId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
