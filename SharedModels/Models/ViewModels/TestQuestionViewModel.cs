using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.ViewModels
{
    public class TestQuestionViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public List<string> Choices { get; set; }
    }
}
