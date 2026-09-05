namespace E_Commerce.Application.DTOs.Wishlists
{
    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string PictureUrl { get; set; } = default!;
        public decimal Price { get; set; }
        public string ProductBrand { get; set; } = default!;
        public string ProductType { get; set; } = default!;
    }
}