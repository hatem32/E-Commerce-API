using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models.Products
{
    // Shared simple form for both ProductBrand and ProductType (they only have a Name)
    public class ProductNameViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name length can not be more than 200")]
        public string Name { get; set; } = default!;
    }

    public class UpdatedProductNameViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name length can not be more than 200")]
        public string Name { get; set; } = default!;
    }
}
