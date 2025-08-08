using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.DTO
{
    public class TestAnswerInput
    {
        public int QuestionId { get; set; }
        public string? SelectedAnswer { get; set; } // nhận index (0..n) hoặc text
    }
}
