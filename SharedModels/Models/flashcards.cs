using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Models
{
    public class flashcards
    {
        [Key]
        public int Id { get; set; }

        public int? GrammarStructureId { get; set; }

        [ForeignKey(nameof(GrammarStructureId))]
        public GrammarStructure? GrammarStructure { get; set; }

        public int? VocabularyId { get; set; }

        [ForeignKey(nameof(VocabularyId))]
        public Vocabulary? Vocabulary { get; set; }

        public int? BaihocId { get; set; }

        [ForeignKey(nameof(BaihocId))]
        public Baihoc? Baihoc { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
