using E_Commerce.Application.Common;
using E_Commerce.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Specifications
{
    
    internal class ProductWithBrandAndTypeSpecifications : BaseSpecifications<Product, int>
    {
        // Get All
        public ProductWithBrandAndTypeSpecifications(ProductQueryParams queryParams)
           : base(p =>
                  (!queryParams.BrandId.HasValue || p.BrandId == queryParams.BrandId.Value)
                  && (!queryParams.TypeId.HasValue || p.TypeId == queryParams.TypeId.Value)
                  && (string.IsNullOrWhiteSpace(queryParams.SearchValue) || p.Name.ToLower().Contains(queryParams.SearchValue!.ToLower())))
           {

            AddInclude(p => p.ProductBrand);
            AddInclude(p => p.ProductType);

            switch (queryParams.Sort)
            {
                case ProductSortingOptions.NameAsc: AddOrderBy(p => p.Name); break;
                case ProductSortingOptions.NameDesc: AddOrderByDescending(p => p.Name); break;
                case ProductSortingOptions.PriceAsc: AddOrderBy(p => p.Price); break;
                case ProductSortingOptions.PriceDesc: AddOrderByDescending(p => p.Price); break;
                default: AddOrderBy(p => p.Id); break;
            }

            ApplyPagination(queryParams.PageSize, queryParams.PageIndex);

        }

        // Get By Id

        public ProductWithBrandAndTypeSpecifications(int id) : base(p => p.Id == id)
        {
            AddInclude(p => p.ProductBrand);
            AddInclude(p => p.ProductType);
        }



    }
}
