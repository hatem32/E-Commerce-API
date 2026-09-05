using E_Commerce.Domain.Entities.Wishlists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Data.Configurations
{
    internal class WishlistItemConfigurations : IEntityTypeConfiguration<WishlistItem>
    {
        public void Configure(EntityTypeBuilder<WishlistItem> builder)
        {
            builder.ToTable("WishlistItems");

            builder.Property(w => w.UserEmail).IsRequired().HasMaxLength(256);

            builder.HasOne(w => w.Product)
                   .WithMany()
                   .HasForeignKey(w => w.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            // A user can only wishlist the same product once.
            builder.HasIndex(w => new { w.UserEmail, w.ProductId }).IsUnique();
        }
    }
}