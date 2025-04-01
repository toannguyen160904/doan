using System.Collections.Generic;

namespace doan.Models
{
    public class LessonViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LevelName { get; set; }
        public List<Vocabulary> tuvung { get; set; } = new List<Vocabulary>();
        public List<GrammarStructure> nguphap { get; set; } = new List<GrammarStructure>();
    }
}
