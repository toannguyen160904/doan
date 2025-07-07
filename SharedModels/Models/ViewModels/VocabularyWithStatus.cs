using Microsoft.AspNetCore.Mvc;

namespace SharedModels.Models.ViewModels
{
    public class VocabularyWithStatus
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string Meaning { get; set; }
        public string LessonName { get; set; }
        public bool IsLearned { get; set; }
    }
}
