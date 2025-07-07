using System.ComponentModel.DataAnnotations;


namespace SharedModels.Models
{
    public class CauTraLoi
    {
        [Key]
        public int Id { get; set; }
        public string ? NoiDung { get; set; }
        public bool  IsCorrect { get; set; }
        public int CauHoiId { get; set; }
        public CauHoi ? CauHoi { get; set; }
    }

}
