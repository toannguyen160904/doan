using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.ViewModels
{
    public class DailyLearningViewModel
    {
        public int CompletedToday { get; set; }
        public int DailyTarget { get; set; }
        public List<Vocabulary> NewWords { get; set; } = new();
        public List<UserVocabularyProgress> ReviewWords { get; set; } = new();
        public List<int> LearnedIds { get; set; } = new();
    }
}
