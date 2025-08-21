using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SharedModels.Models
{
    public class Vocabulary
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập từ vựng.")]
        public string? Tuvung { get; set; }

        public string? PhatAm { get; set; }
        public string? AmHan { get; set; }
        public string? HanTu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nghĩa của từ.")]
        public string? Nghia { get; set; }

        // ✅ FK RÕ RÀNG: dùng LessonId làm khóa ngoại tới bảng Baihoc
        [Required(ErrorMessage = "Vui lòng chọn bài học.")]
        public int LessonId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(LessonId))]          // tránh EF suy đoán sai
        public Baihoc? Lesson { get; set; }     // hoặc đổi tên thành 'Baihoc' nếu bạn muốn theo convention

        // Khuyến nghị để non-nullable nếu luôn có giá trị mặc định
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<UserVocabularyProgress> UserVocabularyProgresses { get; set; } = new List<UserVocabularyProgress>();
    }
}
