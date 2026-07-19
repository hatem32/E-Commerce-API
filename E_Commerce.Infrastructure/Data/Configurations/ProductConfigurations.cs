using E_Commerce.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Infrastructure.Data.Configurations
{
    internal class ProductConfigurations : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasOne(p => p.ProductType)
                   .WithMany()
                   .HasForeignKey(p => p.TypeId);

            builder.HasOne(p => p.ProductBrand)
                   .WithMany()
                   .HasForeignKey(p => p.BrandId);

            builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Property(p => p.Description).HasMaxLength(500);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.PictureUrl).IsRequired().HasMaxLength(500);
        }
    }
}
