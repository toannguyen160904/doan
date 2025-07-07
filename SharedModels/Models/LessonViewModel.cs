using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedModels.Models
{
    public class LessonViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên bài học là bắt buộc")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Cấp độ là bắt buộc")]
        public string LevelName { get; set; }

        public List<Vocabulary> tuvung { get; set; } = new List<Vocabulary>();
        public List<GrammarStructure> nguphap { get; set; } = new List<GrammarStructure>();
    }
}
