using System.ComponentModel.DataAnnotations;
using System;

namespace doan.Models
{
    public class Baihoc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }  // Tên bài học

        [Required]
        [StringLength(255)]
        public string Title { get; set; }  // Tiêu đề chi tiết

        [Required]
        public int LevelId { get; set; }   // FK tự động

        public Level Level { get; set; }   // Navigation Property

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Vocabulary> tuvung { get; set; } = new List<Vocabulary>();
        public List<GrammarStructure> nguphap { get; set; } = new List<GrammarStructure>();
    }
}
