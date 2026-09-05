using AutoMapper;
using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Wishlists;
using E_Commerce.Domain.Contracts;
using E_Commerce.Domain.Entities.Products;
using E_Commerce.Domain.Entities.Wishlists;

namespace E_Commerce.Application.Services
{
    internal class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WishlistService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<WishlistItemDto>>> GetWishlistAsync(string userEmail, CancellationToken ct = default)
        {
            var items = await _unitOfWork.GetRepository<WishlistItem, int>().GetAllAsync(ct);
            var mine = items.Where(w => w.UserEmail == userEmail).ToList();

            // WishlistItem.Product (and its Brand/Type) aren't loaded by the plain
            // GetAllAsync above, so load them explicitly before mapping.
            var productIds = mine.Select(w => w.ProductId).Distinct().ToList();
            var products = await _unitOfWork.GetRepository<Product, int>().GetAllAsync(ct);
            var productsById = products.Where(p => productIds.Contains(p.Id)).ToDictionary(p => p.Id);

            var brands = await _unitOfWork.GetRepository<ProductBrand, int>().GetAllAsync(ct);
            var brandsById = brands.ToDictionary(b => b.Id);
            var types = await _unitOfWork.GetRepository<ProductType, int>().GetAllAsync(ct);
            var typesById = types.ToDictionary(t => t.Id);

            foreach (var item in mine)
            {
                if (productsById.TryGetValue(item.ProductId, out var product))
                {
                    product.ProductBrand = brandsById.GetValueOrDefault(product.BrandId)!;
                    product.ProductType = typesById.GetValueOrDefault(product.TypeId)!;
                    item.Product = product;
                }
            }

            var data = _mapper.Map<IReadOnlyList<WishlistItemDto>>(mine.Where(w => w.Product is not null));
            return Result<IReadOnlyList<WishlistItemDto>>.Ok(data);
        }

        public async Task<Result<WishlistItemDto>> AddToWishlistAsync(string userEmail, int productId, CancellationToken ct = default)
        {
            var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(productId, ct);
            if (product is null)
                return Result<WishlistItemDto>.Fail(Error.NotFound("Product.NotFound", $"Product with id {productId} was not found."));

            var existing = await _unitOfWork.GetRepository<WishlistItem, int>().GetAllAsync(ct);
            var alreadyExists = existing.Any(w => w.UserEmail == userEmail && w.ProductId == productId);

            if (alreadyExists)
                return Result<WishlistItemDto>.Fail(Error.Validation("Wishlist.AlreadyExists", "This product is already in your wishlist."));

            var item = new WishlistItem { UserEmail = userEmail, ProductId = productId };
            _unitOfWork.GetRepository<WishlistItem, int>().Add(item);
            await _unitOfWork.SaveChangesAsync(ct);

            var brand = await _unitOfWork.GetRepository<ProductBrand, int>().GetByIdAsync(product.BrandId, ct);
            var type = await _unitOfWork.GetRepository<ProductType, int>().GetByIdAsync(product.TypeId, ct);
            product.ProductBrand = brand!;
            product.ProductType = type!;
            item.Product = product;

            return _mapper.Map<WishlistItemDto>(item);
        }

        public async Task<Result<bool>> RemoveFromWishlistAsync(string userEmail, int productId, CancellationToken ct = default)
        {
            var items = await _unitOfWork.GetRepository<WishlistItem, int>().GetAllAsync(ct);
            var item = items.FirstOrDefault(w => w.UserEmail == userEmail && w.ProductId == productId);

            if (item is null)
                return Result<bool>.Fail(Error.NotFound("Wishlist.NotFound", "This product is not in your wishlist."));

            _unitOfWork.GetRepository<WishlistItem, int>().Remove(item);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<bool>.Ok(true);
        }
    }
}