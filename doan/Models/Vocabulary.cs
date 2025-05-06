using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace doan.Models
{
    public class Vocabulary
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập từ vựng.")]
        [StringLength(255, ErrorMessage = "Từ vựng không được vượt quá 255 ký tự.")]
        public string? Tuvung { get; set; } // Từ vựng (Kanji/Hiragana/Katakana)

        [StringLength(255)]
        public string? PhatAm { get; set; } // Phát âm (Hiragana)

        [StringLength(255)]
        public string? AmHan { get; set; } // Âm Hán Việt

        [StringLength(255)]
        public string? HanTu { get; set; } // Hán tự

        [Required(ErrorMessage = "Vui lòng nhập nghĩa của từ vựng.")]
        [StringLength(500, ErrorMessage = "Nghĩa không được vượt quá 500 ký tự.")]
        public string? Nghia { get; set; } // Nghĩa tiếng Việt

        [Required(ErrorMessage = "Vui lòng chọn bài học.")]
        public int? LessonId { get; set; } // Khóa ngoại liên kết bài học (nullable để tránh lỗi binding)

        [ForeignKey("LessonId")]
        public Baihoc? Lesson { get; set; } = null!; // Navigation property

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
       
    }
}
