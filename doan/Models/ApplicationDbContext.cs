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
        public DbSet<flashcards> Flashcards { get; set; }
        public DbSet<Diendanmodel> Diendan { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Level data
            modelBuilder.Entity<Level>().HasData(
                new Level { Id = 1, Name = "N5", Description = "Sơ cấp", CreatedAt = new DateTime(2024, 01, 01) },
                new Level { Id = 2, Name = "N4", Description = "Sơ trung cấp", CreatedAt = new DateTime(2024, 01, 01) },
                new Level { Id = 3, Name = "N3", Description = "Trung cấp", CreatedAt = new DateTime(2024, 01, 01) }
            );

            // Configure Baihoc - Level
            modelBuilder.Entity<Baihoc>()
                .HasOne(b => b.Level)
                .WithMany(l => l.Lessons)
                .HasForeignKey(b => b.LevelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Baihoc - Vocabulary
            modelBuilder.Entity<Vocabulary>()
                .HasOne(v => v.Lesson)
                .WithMany(l => l.tuvung)
                .HasForeignKey(v => v.LessonId);

            // Baihoc - GrammarStructure
            modelBuilder.Entity<GrammarStructure>()
                .HasOne(g => g.Lesson)
                .WithMany(l => l.nguphap)
                .HasForeignKey(g => g.LessonId);

            // Quan hệ giữa Vocabulary và Flashcard (1-1)
            modelBuilder.Entity<flashcards>()
                .HasOne(f => f.GrammarStructure)
                .WithOne() // không dùng navigation ở GrammarStructure
                .HasForeignKey<flashcards>(f => f.GrammarStructureId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<flashcards>()
                .HasOne(f => f.Vocabulary)
                .WithOne() // không dùng navigation ở Vocabulary
                .HasForeignKey<flashcards>(f => f.VocabularyId)
                .OnDelete(DeleteBehavior.Restrict);
           
        }

    }
}
