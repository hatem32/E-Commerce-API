using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Application.DTOs.Products
{
    // Used for both create and update - Id is ignored on create.
    public class ProductFormDto
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = default!;

        [Required, StringLength(1000)]
        public string Description { get; set; } = default!;

        // Relative path under /Files (e.g. "images/products/xxx.jpg"), or a full URL.
        [StringLength(500)]
        public string? PictureUrl { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public int BrandId { get; set; }

        public int TypeId { get; set; }
    }
}