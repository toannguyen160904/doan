using Microsoft.AspNetCore.Mvc;

namespace doan.Models
{
    public class UserLearningPlan
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int DailyTarget { get; set; } = 10;
        public int CompletedToday { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public List<Vocabulary> VocabularyList { get; set; } = new List<Vocabulary>(); // Danh sách từ vựng cần học
    }
}
