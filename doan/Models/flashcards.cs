using System.ComponentModel.DataAnnotations;

namespace doan.Models
{
    public class flashcards
    {
        [Key]
        public int Id { get; set; }

        public int? GrammarStructureId { get; set; }
        public GrammarStructure? GrammarStructure { get; set; }

        public int? VocabularyId { get; set; }
        public Vocabulary? Vocabulary { get; set; }
        public int BaihocId { get; set; }
        public Baihoc? Baihoc { get; set; } // DẤU HỎI: Cho phép null để tránh lỗi binding
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    }
}
