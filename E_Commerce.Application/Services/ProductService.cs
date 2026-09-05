using AutoMapper;
using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Products;
using E_Commerce.Application.Specifications;
using E_Commerce.Domain.Contracts;
using E_Commerce.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Services
{
    internal class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }
        public async Task<Result<IReadOnlyList<BrandDto>>> GetAllBrandsAsync(CancellationToken ct = default)
        {
            var brands = await _unitOfWork.GetRepository<ProductBrand, int>().GetAllAsync(ct);
            var data = _mapper.Map<IReadOnlyList<BrandDto>>(brands);
            return Result<IReadOnlyList<BrandDto>>.Ok(data);
        }

        public async Task<Result<PaginatedResult<ProductDto>>> GetAllProductsAsync(ProductQueryParams queryParams, CancellationToken ct = default)
        {
            var Spec = new ProductWithBrandAndTypeSpecifications(queryParams);
            var products = await _unitOfWork.GetRepository<Product, int>().GetAllAsync(Spec);
            var data = _mapper.Map<IReadOnlyList<ProductDto>>(products);
            var CountSpec = new ProductCountSpecifications(queryParams);
            var CountOfAllProducts = await _unitOfWork.GetRepository<Product, int>().CountAsync(CountSpec);
            var result = new PaginatedResult<ProductDto>(queryParams.PageIndex, queryParams.PageSize, CountOfAllProducts, data);
            return Result<PaginatedResult<ProductDto>>.Ok(result);
        }

        public async Task<Result<IReadOnlyList<TypeDto>>> GetAllTypesAsync(CancellationToken ct = default)
        {
            var types = await _unitOfWork.GetRepository<ProductType, int>().GetAllAsync(ct);
            var data = _mapper.Map<IReadOnlyList<TypeDto>>(types);
            return Result<IReadOnlyList<TypeDto>>.Ok(data);
        }

        public async Task<Result<ProductDto>> GetProductByIdAsync(int id, CancellationToken ct = default)
        {
            var spec = new ProductWithBrandAndTypeSpecifications(id);
            var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(spec, ct);

            if (product is null)
                return Result<ProductDto>.Fail(Error.NotFound("Product.NotFound", $"Product with id {id} was not found."));

            return _mapper.Map<ProductDto>(product);
        }

        // ---------------- Admin: Products ----------------

        public async Task<Result<ProductDto>> CreateProductAsync(ProductFormDto dto, CancellationToken ct = default)
        {
            var brandRepo = _unitOfWork.GetRepository<ProductBrand, int>();
            var typeRepo = _unitOfWork.GetRepository<ProductType, int>();

            if (await brandRepo.GetByIdAsync(dto.BrandId, ct) is null)
                return Result<ProductDto>.Fail(Error.NotFound("Brand.NotFound", $"Brand with id {dto.BrandId} was not found."));

            if (await typeRepo.GetByIdAsync(dto.TypeId, ct) is null)
                return Result<ProductDto>.Fail(Error.NotFound("Type.NotFound", $"Type with id {dto.TypeId} was not found."));

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                PictureUrl = dto.PictureUrl ?? string.Empty,
                Price = dto.Price,
                BrandId = dto.BrandId,
                TypeId = dto.TypeId
            };

            var productRepo = _unitOfWork.GetRepository<Product, int>();
            productRepo.Add(product);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cacheService.RemoveByPrefixAsync("/api/products", ct);

            return await GetProductByIdAsync(product.Id, ct);
        }

        public async Task<Result<ProductDto>> UpdateProductAsync(int id, ProductFormDto dto, CancellationToken ct = default)
        {
            var productRepo = _unitOfWork.GetRepository<Product, int>();
            var product = await productRepo.GetByIdAsync(id, ct);

            if (product is null)
                return Result<ProductDto>.Fail(Error.NotFound("Product.NotFound", $"Product with id {id} was not found."));

            var brandRepo = _unitOfWork.GetRepository<ProductBrand, int>();
            var typeRepo = _unitOfWork.GetRepository<ProductType, int>();

            if (await brandRepo.GetByIdAsync(dto.BrandId, ct) is null)
                return Result<ProductDto>.Fail(Error.NotFound("Brand.NotFound", $"Brand with id {dto.BrandId} was not found."));

            if (await typeRepo.GetByIdAsync(dto.TypeId, ct) is null)
                return Result<ProductDto>.Fail(Error.NotFound("Type.NotFound", $"Type with id {dto.TypeId} was not found."));

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.BrandId = dto.BrandId;
            product.TypeId = dto.TypeId;

            if (!string.IsNullOrWhiteSpace(dto.PictureUrl))
                product.PictureUrl = dto.PictureUrl;

            productRepo.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cacheService.RemoveByPrefixAsync("/api/products", ct);

            return await GetProductByIdAsync(id, ct);
        }

        public async Task<Result<bool>> DeleteProductAsync(int id, CancellationToken ct = default)
        {
            var productRepo = _unitOfWork.GetRepository<Product, int>();
            var product = await productRepo.GetByIdAsync(id, ct);

            if (product is null)
                return Result<bool>.Fail(Error.NotFound("Product.NotFound", $"Product with id {id} was not found."));

            productRepo.Remove(product);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cacheService.RemoveByPrefixAsync("/api/products", ct);

            return Result<bool>.Ok(true);
        }

        // ---------------- Admin: Brands ----------------

        public async Task<Result<BrandDto>> CreateBrandAsync(NameFormDto dto, CancellationToken ct = default)
        {
            var brandRepo = _unitOfWork.GetRepository<ProductBrand, int>();
            var existing = await brandRepo.GetAllAsync(ct);

            if (existing.Any(b => b.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
                return Result<BrandDto>.Fail(Error.Validation("Brand.AlreadyExists", $"A brand named '{dto.Name}' already exists."));

            var brand = new ProductBrand { Name = dto.Name };
            brandRepo.Add(brand);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cacheService.RemoveByPrefixAsync("/api/products", ct);

            return _mapper.Map<BrandDto>(brand);
        }

        public async Task<Result<BrandDto>> UpdateBrandAsync(int id, NameFormDto dto, CancellationToken ct = default)
        {
            var brandRepo = _unitOfWork.GetRepository<ProductBrand, int>();
            var brand = await brandRepo.GetByIdAsync(id, ct);

            if (brand is null)
                return Result<BrandDto>.Fail(Error.NotFound("Brand.NotFound", $"Brand with id {id} was not found."));

            brand.Name = dto.Name;
            brandRepo.Update(brand);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cacheService.RemoveByPrefixAsync("/api/products", ct);

            return _mapper.Map<BrandDto>(brand);
        }

        public async Task<Result<bool>> DeleteBrandAsync(int id, CancellationToken ct = default)
        {
            var brandRepo = _unitOfWork.GetRepository<ProductBrand, int>();
            var brand = await brandRepo.GetByIdAsync(id, ct);

            if (brand is null)
                return Result<bool>.Fail(Error.NotFound("Brand.NotFound", $"Brand with id {id} was not found."));

            var products = await _unitOfWork.GetRepository<Product, int>().GetAllAsync(ct);
            if (products.Any(p => p.BrandId == id))
                return Result<bool>.Fail(Error.Validation("Brand.InUse", "This brand is used by one or more products and can't be deleted."));

            brandRepo.Remove(brand);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cacheService.RemoveByPrefixAsync("/api/products", ct);

            return Result<bool>.Ok(true);
        }

        // ---------------- Admin: Types ----------------

        public async Task<Result<TypeDto>> CreateTypeAsync(NameFormDto dto, CancellationToken ct = default)
        {
            var typeRepo = _unitOfWork.GetRepository<ProductType, int>();
            var existing = await typeRepo.GetAllAsync(ct);

            if (existing.Any(t => t.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
                return Result<TypeDto>.Fail(Error.Validation("Type.AlreadyExists", $"A type named '{dto.Name}' already exists."));

            var type = new ProductType { Name = dto.Name };
            typeRepo.Add(type);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cacheService.RemoveByPrefixAsync("/api/products", ct);

            return _mapper.Map<TypeDto>(type);
        }

        public async Task<Result<TypeDto>> UpdateTypeAsync(int id, NameFormDto dto, CancellationToken ct = default)
        {
            var typeRepo = _unitOfWork.GetRepository<ProductType, int>();
            var type = await typeRepo.GetByIdAsync(id, ct);

            if (type is null)
                return Result<TypeDto>.Fail(Error.NotFound("Type.NotFound", $"Type with id {id} was not found."));

            type.Name = dto.Name;
            typeRepo.Update(type);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cacheService.RemoveByPrefixAsync("/api/products", ct);

            return _mapper.Map<TypeDto>(type);
        }

        public async Task<Result<bool>> DeleteTypeAsync(int id, CancellationToken ct = default)
        {
            var typeRepo = _unitOfWork.GetRepository<ProductType, int>();
            var type = await typeRepo.GetByIdAsync(id, ct);

            if (type is null)
                return Result<bool>.Fail(Error.NotFound("Type.NotFound", $"Type with id {id} was not found."));

            var products = await _unitOfWork.GetRepository<Product, int>().GetAllAsync(ct);
            if (products.Any(p => p.TypeId == id))
                return Result<bool>.Fail(Error.Validation("Type.InUse", "This type is used by one or more products and can't be deleted."));

            typeRepo.Remove(type);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cacheService.RemoveByPrefixAsync("/api/products", ct);

            return Result<bool>.Ok(true);
        }
    }
}