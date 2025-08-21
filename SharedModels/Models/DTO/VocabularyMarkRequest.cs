using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedModels.Models.DTO
{
    public class VocabularyMarkRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        [JsonPropertyName("vocabId")]
        public int VocabId { get; set; }
        [JsonPropertyName("vocabularyId")]
        public int VocabularyId { get => VocabId; set => VocabId = value; }

    }
}
