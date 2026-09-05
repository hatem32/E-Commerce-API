using E_Commerce.Application.Common;
using E_Commerce.Application.DTOs.Wishlists;

namespace E_Commerce.Application.Contracts
{
    public interface IWishlistService
    {
        Task<Result<IReadOnlyList<WishlistItemDto>>> GetWishlistAsync(string userEmail, CancellationToken ct = default);
        Task<Result<WishlistItemDto>> AddToWishlistAsync(string userEmail, int productId, CancellationToken ct = default);
        Task<Result<bool>> RemoveFromWishlistAsync(string userEmail, int productId, CancellationToken ct = default);
    }
}