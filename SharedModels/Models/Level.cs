using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace SharedModels.Models
{
    public class Level
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Name { get; set; }  // Tên Level (N5, N4, N3)

        [StringLength(500)]
        [Required]
        public string Description { get; set; } = "";  // Mô tả cấp độ

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public ICollection<Baihoc> Lessons { get; set; } = new List<Baihoc>();

    }
}
