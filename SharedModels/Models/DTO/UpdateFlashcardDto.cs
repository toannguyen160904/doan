using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.DTO
{
    public class UpdateFlashcardDto
    {
        public int? BaihocId { get; set; }
        public int? VocabularyId { get; set; }
        public int? GrammarStructureId { get; set; }
    }
}
