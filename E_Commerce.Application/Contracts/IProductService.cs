using E_Commerce.Application.Common;
using E_Commerce.Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Contracts
{
    public interface IProductService
    {
        Task<Result<PaginatedResult<ProductDto>>> GetAllProductsAsync(ProductQueryParams queryParams, CancellationToken ct = default);
        Task<Result<IReadOnlyList<BrandDto>>> GetAllBrandsAsync(CancellationToken ct = default);
        Task<Result<IReadOnlyList<TypeDto>>> GetAllTypesAsync(CancellationToken ct = default);
        Task<Result<ProductDto>> GetProductByIdAsync(int id, CancellationToken ct = default);

        // Admin - Products
        Task<Result<ProductDto>> CreateProductAsync(ProductFormDto dto, CancellationToken ct = default);
        Task<Result<ProductDto>> UpdateProductAsync(int id, ProductFormDto dto, CancellationToken ct = default);
        Task<Result<bool>> DeleteProductAsync(int id, CancellationToken ct = default);

        // Admin - Brands
        Task<Result<BrandDto>> CreateBrandAsync(NameFormDto dto, CancellationToken ct = default);
        Task<Result<BrandDto>> UpdateBrandAsync(int id, NameFormDto dto, CancellationToken ct = default);
        Task<Result<bool>> DeleteBrandAsync(int id, CancellationToken ct = default);

        // Admin - Types
        Task<Result<TypeDto>> CreateTypeAsync(NameFormDto dto, CancellationToken ct = default);
        Task<Result<TypeDto>> UpdateTypeAsync(int id, NameFormDto dto, CancellationToken ct = default);
        Task<Result<bool>> DeleteTypeAsync(int id, CancellationToken ct = default);
    }
}