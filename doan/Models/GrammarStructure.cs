using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace doan.Models
{
    public class GrammarStructure
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string CongThuc { get; set; } // Công thức ngữ pháp (〜ている, 〜たことがある)Structure

        [Required]
        public string GiaiThich { get; set; } // Giải thích cách dùngExplanation

        public string CauViDu { get; set; } // Câu ví dụ minh họaExample

        [Required]
        public int LessonId { get; set; } // Khóa ngoại liên kết với bảng Lesson

        [ForeignKey("LessonId")]
        public Baihoc Lesson { get; set; } // Navigation property

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
