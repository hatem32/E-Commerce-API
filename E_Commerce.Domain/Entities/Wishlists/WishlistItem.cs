using E_Commerce.Domain.Common;
using E_Commerce.Domain.Entities.Products;

namespace E_Commerce.Domain.Entities.Wishlists
{
    public class WishlistItem : BaseEntity<int>
    {
        // Identity lives in a separate DbContext/database, so - same pattern as
        // Order.BuyerEmail - we store the owning user's email as a plain string
        // rather than a real foreign key.
        public string UserEmail { get; set; } = default!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}