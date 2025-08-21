using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.DTO
{
    public class LessonCreateDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int LevelId { get; set; }
        public int? Order { get; set; }
        public bool? IsPreview { get; set; }
    }
}
