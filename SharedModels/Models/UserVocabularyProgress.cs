using SharedModels.Models;
using Microsoft.AspNetCore.Mvc;

namespace SharedModels.Models
{
    public class UserVocabularyProgress
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int VocabularyId { get; set; }
        public Vocabulary Vocabulary { get; set; }  // Navigation property to Vocabulary

        public bool IsLearned { get; set; }
        public DateTime? LearnedDate { get; set; }

        public DateTime? NextReviewDate { get; set; }  // The next review date for spaced repetition
        public int ReviewLevel { get; set; } = 0;  // 0 = new, 1 = review in 1 day, 2 = review in 3 days, etc.
    }

}
