using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace doan.Models.ViewModels
{
    public class UserAnswerViewModel
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string SelectedAnswer { get; set; }
    }
}
