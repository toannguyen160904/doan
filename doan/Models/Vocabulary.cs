using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace doan.Models
{
    public class Vocabulary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string? Tuvung { get; set; } // Từ vựng (Kanji/Hiragana/Katakana)Word

        [StringLength(255)]
        public string? PhatAm { get; set; } // Phát âm (Hiragana)Pronunciation

        [StringLength(255)]
        public string? AmHan { get; set; } // Âm Hán ViệtSinoVietnamese

        [StringLength(255)]
        public string? HanTu { get; set; } // Hán tự

        [Required]
        [StringLength(500)]
        public string? Nghia { get; set; } // Nghĩa tiếng ViệtMeaning

        [Required]
        public int LessonId { get; set; } // Khóa ngoại liên kết bảng Lesson

        [ForeignKey("LessonId")]
        public Baihoc Lesson { get; set; } = null!;// Navigation property

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
