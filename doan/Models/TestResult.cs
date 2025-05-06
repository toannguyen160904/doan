using Microsoft.AspNetCore.Mvc;

namespace doan.Models
{
    public class TestResult
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int TotalScore { get; set; }

        public int CorrectAnswers { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public string SuggestedLevel { get; set; } // Gợi ý N5, N4...
    }

}
