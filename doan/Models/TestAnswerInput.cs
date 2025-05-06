using Microsoft.AspNetCore.Mvc;

namespace doan.Models
{
    public class TestAnswerInput
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
    }
}
