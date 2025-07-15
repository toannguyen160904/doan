using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.ViewModels
{
    public class LevelLessonsViewModel
    {
        public string SelectedLevelName { get; set; }
        public List<Level> AllLevels { get; set; }
        public List<Baihoc> Lessons { get; set; }
    }
}
