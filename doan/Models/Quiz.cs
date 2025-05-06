using System.ComponentModel.DataAnnotations;

namespace doan.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }
        public int  BaihocId { get; set; }
        public Baihoc? Baihoc { get; set; }
        public ICollection<CauHoi> CauHois { get; set; } = new List<CauHoi>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
