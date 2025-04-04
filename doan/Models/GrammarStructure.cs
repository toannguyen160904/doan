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
        public string CongThuc { get; set; } // Công thức ngữ pháp (〜ている, 〜たことがある)

        [Required]
        public string GiaiThich { get; set; } // Giải thích cách dùng

        public string CauViDu { get; set; } // Câu ví dụ minh họa

        [Required]
        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Baihoc Lesson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Aliases for compatibility in Razor Views
        [NotMapped]
        public string Structure
        {
            get => CongThuc;
            set => CongThuc = value;
        }

        [NotMapped]
        public string Explanation
        {
            get => GiaiThich;
            set => GiaiThich = value;
        }
    }
}
