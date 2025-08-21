using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.DTO
{
    public class CreateFlashcardDto
    {
        public int? VocabularyId { get; set; }
        public int? GrammarStructureId { get; set; }
    }
}
