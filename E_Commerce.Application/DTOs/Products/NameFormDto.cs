using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Application.DTOs.Products
{
    // Shared create/update form for ProductBrand and ProductType (both are just a Name).
    public class NameFormDto
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = default!;
    }
}