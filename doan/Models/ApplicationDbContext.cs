using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace doan.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }  // ✅ Đúng
        public DbSet<Level> Levels { get; set; }
        public DbSet<Baihoc> Baihoc { get; set; }
        public DbSet<Vocabulary> tuvung { get; set; }
        public DbSet<GrammarStructure> nguphap { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quan hệ Baihoc - Level
            modelBuilder.Entity<Baihoc>()
                .HasOne(b => b.Level)
                .WithMany()
                .HasForeignKey(b => b.LevelId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa bài học thì xóa cả quan hệ

            // Quan hệ Baihoc - Vocabulary
            modelBuilder.Entity<Baihoc>()
                .HasMany(b => b.tuvung)
                .WithOne(v => v.Lesson)
                .HasForeignKey(v => v.LessonId);

            // Quan hệ Baihoc - Grammar
            modelBuilder.Entity<Baihoc>()
                .HasMany(b => b.nguphap)
                .WithOne(g => g.Lesson)
                .HasForeignKey(g => g.LessonId);
        }

    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }


}
