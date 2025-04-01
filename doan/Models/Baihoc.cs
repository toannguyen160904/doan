using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace doan.Models
{
    public class Baihoc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }  // Tên bài học (VD: Chào hỏi, Giới thiệu bản thân)

        [Required]
        public int LevelId { get; set; } // Khóa ngoại liên kết bảng Level

        [ForeignKey("LevelId")]
        public Level Level { get; set; } // Navigation property

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public List<Baihoc> Lessons { get; set; } = new List<Baihoc>();

        public ICollection<Vocabulary> tuvung { get; set; } = new List<Vocabulary>(); // Danh sách từ vựng
        public List<GrammarStructure> nguphap { get; set; } = new List<GrammarStructure>();
    }
}
