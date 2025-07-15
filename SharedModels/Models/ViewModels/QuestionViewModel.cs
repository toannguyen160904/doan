using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.ViewModels
{
    public class QuestionViewModel
    {
        public int Id { get; set; }
        public string NoiDung { get; set; }
        public List<AnswerViewModel> CauTraLois { get; set; }
    }

}
