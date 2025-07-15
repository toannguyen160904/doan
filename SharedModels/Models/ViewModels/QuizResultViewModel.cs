using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.ViewModels
{
    public class QuizResultViewModel
    {
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public Dictionary<int, int> CorrectAnswers { get; set; }
        public Dictionary<int, int> UserAnswers { get; set; }
    }
}
