// File: ApplicationDbContext.cs
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
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<CauHoi> CauHois { get; set; }
        public DbSet<CauTraLoi> CauTraLois { get; set; }
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

            //Baihoc - Quiz
            modelBuilder.Entity<Baihoc>()
               .HasOne(b => b.Quiz)
               .WithOne(q => q.Baihoc)
               .HasForeignKey<Quiz>(q => q.BaihocId)
               .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa Quiz và CauHoi (1-nhiều)
            modelBuilder.Entity<CauHoi>()
                .HasOne(ch => ch.Quiz)
                .WithMany(q => q.CauHois)
                .HasForeignKey(ch => ch.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa CauHoi và CauTraLoi (1-nhiều)
            modelBuilder.Entity<CauTraLoi>()
                .HasOne(ctl => ctl.CauHoi)
                .WithMany(ch => ch.CauTraLois)
                .HasForeignKey(ctl => ctl.CauHoiId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
