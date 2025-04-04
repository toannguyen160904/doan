using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace doan.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public required string Name { get; set; }

        public List<Product>? Products { get; set; }
    }
}
