using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace doan.Models
{
    public class GrammarStructure
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập cấu trúc.")]
        [StringLength(255)]
        public string CongThuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giải thích.")]
        public string GiaiThich { get; set; }

        public string CauViDu { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bài học.")]
        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Baihoc? Lesson { get; set; } // DẤU HỎI: Cho phép null để tránh lỗi binding


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
