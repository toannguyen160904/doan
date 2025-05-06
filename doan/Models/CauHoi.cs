using System.ComponentModel.DataAnnotations;
namespace doan.Models
{
    public class CauHoi
    {
        [Key]
        public int Id { get; set; }
        public string NoiDung { get; set; }
        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }
        public ICollection<CauTraLoi> CauTraLois { get; set; } = new List<CauTraLoi>();
    }

}
