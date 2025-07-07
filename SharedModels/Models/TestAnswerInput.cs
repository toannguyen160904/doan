using Microsoft.AspNetCore.Mvc;

namespace SharedModels.Models
{
    public class TestAnswerInput
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
    }
}
