using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.ViewModels
{
    public class QuizForStudentViewModel
    {
        public int Id { get; set; }
        public string TenQuiz { get; set; }
        public int BaihocId { get; set; }
        public List<QuestionViewModel> CauHois { get; set; }
    }
}
