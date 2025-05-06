using Microsoft.AspNetCore.Mvc;

namespace doan.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }

        public int TestResultId { get; set; }
        public TestResult TestResult { get; set; }

        public int TestQuestionId { get; set; }
        public TestQuestion Question { get; set; }

        public string SelectedAnswer { get; set; }

        public bool IsCorrect => SelectedAnswer == Question.CorrectAnswer;
    }

}
