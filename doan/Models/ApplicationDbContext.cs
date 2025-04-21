using doan.Controllers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace doan.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Baihoc> Baihoc { get; set; }
        public DbSet<Vocabulary> tuvung { get; set; }
        public DbSet<GrammarStructure> nguphap { get; set; }
        public DbSet<UserLearningPlan> UserLearningPlans { get; set; }
        public DbSet<UserVocabularyProgress> UserVocabularyProgresses { get; set; }


        public DbSet<TestQuestion> TestQuestions { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserVocabularyProgress>()
                .HasOne(p => p.Vocabulary)
                .WithMany(v => v.UserVocabularyProgresses)  // Vocabulary has many UserVocabularyProgresses
                .HasForeignKey(p => p.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade);  // Handle cascading deletes if needed

            // Seed Level data
            modelBuilder.Entity<Level>().HasData(
                new Level { Id = 1, Name = "N5", Description = "Sơ cấp", CreatedAt = new DateTime(2024, 01, 01) },
                new Level { Id = 2, Name = "N4", Description = "Sơ trung cấp", CreatedAt = new DateTime(2024, 01, 01) },
                new Level { Id = 3, Name = "N3", Description = "Trung cấp", CreatedAt = new DateTime(2024, 01, 01) }
            );

            // Configure Baihoc - Level relationship
            modelBuilder.Entity<Baihoc>()
                .HasOne(b => b.Level)
                .WithMany(l => l.Lessons)
                .HasForeignKey(b => b.LevelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Baihoc - Vocabulary relationship
            modelBuilder.Entity<Vocabulary>()
                .HasOne(v => v.Lesson)
                .WithMany(l => l.tuvung)
                .HasForeignKey(v => v.LessonId);

            // Baihoc - GrammarStructure relationship
            modelBuilder.Entity<GrammarStructure>()
                .HasOne(g => g.Lesson)
                .WithMany(l => l.nguphap)
                .HasForeignKey(g => g.LessonId);
        }
    }
}
