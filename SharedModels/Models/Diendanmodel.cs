
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace SharedModels.Models
{
    public class Diendanmodel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int BaiHocId { get; set; }
        [Required]
        [StringLength(255)]
        public string TieuDe { get; set; }

        public string NoiDung { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [ValidateNever]
        public Baihoc Baihoc { get; set; }
        [Required]
        public string UserId { get; set; }   

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }  
    }
}
