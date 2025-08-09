using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace SharedModels.Models
{
    public class Baihoc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public bool IsDeleted { get; set; } = false;

        [Required]
        public int LevelId { get; set; }

        [ValidateNever] // 👈 Bỏ qua validation cho navigation property
        public Level Level { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Range(1, int.MaxValue, ErrorMessage = "Order phải >= 1")]
        public int Order { get; set; } = 1;
        public bool IsPreview { get; set; } = false;

        public ICollection<Vocabulary> tuvung { get; set; } = new List<Vocabulary>();
        public List<GrammarStructure> nguphap { get; set; } = new List<GrammarStructure>();

        public ICollection<flashcards> Flashcards { get; set; } = new List<flashcards>();
        public ICollection<Diendanmodel> Diendan { get; set; } = new List<Diendanmodel>();

        public Quiz? Quiz { get; set; }

    }
}