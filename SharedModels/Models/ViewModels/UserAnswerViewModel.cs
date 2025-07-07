using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SharedModels.Models.ViewModels
{
    public class UserAnswerViewModel
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string SelectedAnswer { get; set; }
    }
}
