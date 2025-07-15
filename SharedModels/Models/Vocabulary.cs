using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
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

        [Required(ErrorMessage = "Vui lòng chọn bài học.")]
        public int LessonId { get; set; }
        [JsonIgnore]
        public Baihoc? Lesson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ✅ Gán giá trị mặc định
        [JsonIgnore]

        public ICollection<UserVocabularyProgress> UserVocabularyProgresses { get; set; } = new List<UserVocabularyProgress>();

        

 
       

    }
}
