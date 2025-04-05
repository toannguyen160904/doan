using System.ComponentModel.DataAnnotations;
using doan.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace doan.Models
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

        [Required]
        public int LevelId { get; set; }

        [ValidateNever] // 👈 Bỏ qua validation cho navigation property
        public Level Level { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Vocabulary> tuvung { get; set; } = new List<Vocabulary>();
        public List<GrammarStructure> nguphap { get; set; } = new List<GrammarStructure>();
    }
}