using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models.Products
{
    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200)]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000)]
        public string Description { get; set; } = default!;

        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }

        // Kept so an existing/typed image path or full URL can still be used,
        // and so the current image can be shown when editing a product.
        [StringLength(500)]
        [Display(Name = "Picture URL")]
        public string? PictureUrl { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Please select a brand")]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Please select a product type")]
        [Display(Name = "Product Type")]
        public int TypeId { get; set; }

        public IEnumerable<SelectListItem> Brands { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Types { get; set; } = new List<SelectListItem>();
    }
}