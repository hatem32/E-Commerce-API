using E_Commerce.Application.Common;
using E_Commerce.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Specifications
{
    internal class ProductCountSpecifications : BaseSpecifications<Product, int>
    {
        public ProductCountSpecifications(ProductQueryParams queryParams)
            : base(p => (!queryParams.BrandId.HasValue || p.BrandId == queryParams.BrandId)
                && (!queryParams.TypeId.HasValue || p.TypeId == queryParams.TypeId)
                && (string.IsNullOrWhiteSpace(queryParams.SearchValue) || p.Name.Contains(queryParams.SearchValue!.ToLower())))
        {
        }
    }
}
