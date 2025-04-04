using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace doan.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public required string Name { get; set; }

        [Range(0.01, 10000.00)]
        public int Price { get; set; }

        public required string Description { get; set; }

        public string? ImageUrl { get; set; }

        public List<ProductImage>? Images { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
