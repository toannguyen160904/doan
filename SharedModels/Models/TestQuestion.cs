using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;          
using System.Collections.Generic;                     
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SharedModels.Models
{
    public class TestQuestion
    {
        public int Id { get; set; }

        [Required]
        public string QuestionText { get; set; }

        [Required]
        public string CorrectAnswer { get; set; }

        private List<string> _choices = new();

        [NotMapped]
        public List<string> Choices
        {
            get => _choices;
            set => _choices = value ?? new();
        }

        public string ChoicesJson
        {
            get => JsonSerializer.Serialize(_choices);
            set => _choices = string.IsNullOrWhiteSpace(value)
                ? new()
                : JsonSerializer.Deserialize<List<string>>(value) ?? new();
        }

        public string Topic { get; set; }
        public string Level { get; set; }
        public int Score { get; set; } = 1;
        public string? Explanation { get; set; }
        public bool IsActive { get; set; } = true;
    }

}
