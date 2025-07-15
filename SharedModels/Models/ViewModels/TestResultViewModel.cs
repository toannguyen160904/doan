using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.ViewModels
{
    public class TestResultViewModel
    {
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public string SuggestedLevel { get; set; }
    }
}
