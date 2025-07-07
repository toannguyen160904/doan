using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;          
using System.Collections.Generic;                     
using Microsoft.EntityFrameworkCore;                   

namespace SharedModels.Models
{
    public class TestQuestion
    {
        public int Id { get; set; }

        [Required]
        public string QuestionText { get; set; }

        [Required]
        public string CorrectAnswer { get; set; }

        [NotMapped]
        public List<string> Choices { get; set; } = new();

        public string ChoicesJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(Choices);
            set => Choices = string.IsNullOrEmpty(value)
                ? new List<string>()
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(value);
        }

        public string Topic { get; set; }
        public string Level { get; set; }
        public int Score { get; set; } = 1;
        public string? Explanation { get; set; }
        public bool IsActive { get; set; } = true;
    }

}
