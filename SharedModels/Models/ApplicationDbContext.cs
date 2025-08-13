
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using System;
using SharedModels.Models;

namespace SharedModels.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet cho các thực thể
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


        public DbSet<flashcards> Flashcards { get; set; }
        public DbSet<Diendanmodel> Diendan { get; set; }



        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<CauHoi> CauHois { get; set; }
        public DbSet<CauTraLoi> CauTraLois { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("public");
            modelBuilder.Entity<Level>().ToTable("Levels");

            // Cấu hình mối quan hệ: UserVocabularyProgress - Vocabulary
            modelBuilder.Entity<UserVocabularyProgress>()
                .HasOne(p => p.Vocabulary)
                .WithMany(v => v.UserVocabularyProgresses)
                .HasForeignKey(p => p.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed dữ liệu Level
            modelBuilder.Entity<Level>().HasData(
    new Level { Id = 1, Name = "N5", Description = "Sơ cấp", CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc) },
    new Level { Id = 2, Name = "N4", Description = "Sơ trung cấp", CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc) },
    new Level { Id = 3, Name = "N3", Description = "Trung cấp", CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc) }
);


            // Quan hệ: Baihoc - Level (1 - nhiều)
            modelBuilder.Entity<Baihoc>()
                .HasOne(b => b.Level)
                .WithMany(l => l.Lessons)
                .HasForeignKey(b => b.LevelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Baihoc - Unique theo tên + level
            modelBuilder.Entity<Baihoc>()
                .HasIndex(b => new { b.Name, b.LevelId })
                .IsUnique();

            // Baihoc - Index theo LevelId, Order (để sắp xếp)
            modelBuilder.Entity<Baihoc>()
            .HasIndex(b => new { b.LevelId, b.Order })
             .IsUnique();

            // Baihoc - Index theo LevelId, IsDeleted, Order
            modelBuilder.Entity<Baihoc>()
            .HasIndex(b => new { b.LevelId, b.IsDeleted, b.Order });

            // Vocabulary - Unique theo Hán tự + Lesson
            modelBuilder.Entity<Vocabulary>()
                .HasIndex(v => new { v.HanTu, v.LessonId })
                .IsUnique();

            modelBuilder.Entity<Baihoc>()
                .HasIndex(b => new { b.LevelId, b.Order })
                .IsUnique();

            modelBuilder.Entity<Baihoc>()
                .Property(b => b.IsPreview)
                .HasDefaultValue(false);

            modelBuilder.Entity<Baihoc>()
                .HasQueryFilter(b => !b.IsDeleted);

            // Vocabulary - Baihoc (optional để tránh lỗi khi Baihoc bị filter)
            modelBuilder.Entity<Vocabulary>()
                .HasOne(v => v.Lesson)
                .WithMany(l => l.tuvung)
                .HasForeignKey(v => v.LessonId)
                .IsRequired(false); // ✅ optional

            // GrammarStructure - Baihoc (optional)
            modelBuilder.Entity<GrammarStructure>()
                .HasOne(g => g.Lesson)
                .WithMany(l => l.nguphap)
                .HasForeignKey(g => g.LessonId)
                .IsRequired(false); // ✅ optional

            // Flashcards - GrammarStructure (1-1, optional)
            // Quan hệ Baihoc - Flashcards
            //modelBuilder.Entity<flashcards>()
            //    .HasOne(f => f.Baihoc)
            //    .WithMany(b => b.Flashcards)
            //    .HasForeignKey(f => f.BaihocId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //// Flashcards - GrammarStructure (1-1 optional)
            //modelBuilder.Entity<flashcards>()
            //    .HasOne(f => f.GrammarStructure)
            //    .WithOne()
            //    .HasForeignKey<flashcards>(f => f.GrammarStructureId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //// Flashcards - Vocabulary (1-1 optional)
            //modelBuilder.Entity<flashcards>()
            //    .HasOne(f => f.Vocabulary)
            //    .WithOne()
            //    .HasForeignKey<flashcards>(f => f.VocabularyId)
            //    .OnDelete(DeleteBehavior.Restrict);


            // Baihoc - Quiz (1-1, optional)
            modelBuilder.Entity<Baihoc>()
                .HasOne(b => b.Quiz)
                .WithOne(q => q.Baihoc)
                .HasForeignKey<Quiz>(q => q.BaihocId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // ✅ optional

            // Quiz - CauHoi (1-n)
            modelBuilder.Entity<CauHoi>()
                .HasOne(ch => ch.Quiz)
                .WithMany(q => q.CauHois)
                .HasForeignKey(ch => ch.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<flashcards>().Property(f => f.GrammarStructureId).HasColumnType("integer");
            modelBuilder.Entity<flashcards>().Property(f => f.VocabularyId).HasColumnType("integer");
            modelBuilder.Entity<flashcards>().Property(f => f.BaihocId).HasColumnType("integer");
            // CauHoi - CauTraLoi (1-n)
            modelBuilder.Entity<CauTraLoi>()
                .HasOne(ctl => ctl.CauHoi)
                .WithMany(ch => ch.CauTraLois)
                .HasForeignKey(ctl => ctl.CauHoiId)
                .OnDelete(DeleteBehavior.Cascade);

            // Baihoc - Diendanmodel (optional nếu có quan hệ)
            modelBuilder.Entity<Diendanmodel>()
                .HasOne(d => d.Baihoc)
                .WithMany() // Nếu có navigation property ngược thì .WithMany(b => b.Diendan)
                .HasForeignKey(d => d.BaiHocId)
                .IsRequired(false); // ✅ optional

            // ✅ Global query filter cho Baihoc (đặt sau cùng)
            modelBuilder.Entity<Baihoc>().HasQueryFilter(b => !b.IsDeleted);
        }

    }
}
