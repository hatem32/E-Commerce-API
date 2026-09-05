using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Wishlists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API.Controllers
{
    [Authorize]
    public class WishlistController : ApiBaseController
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<WishlistItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<WishlistItemDto>>> GetWishlist(CancellationToken ct)
            => ToActionResult(await _wishlistService.GetWishlistAsync(GetEmailFromToken(), ct));

        [HttpPost("{productId:int}")]
        [ProducesResponseType(typeof(WishlistItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WishlistItemDto>> AddToWishlist(int productId, CancellationToken ct)
            => ToActionResult(await _wishlistService.AddToWishlistAsync(GetEmailFromToken(), productId, ct));

        [HttpDelete("{productId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> RemoveFromWishlist(int productId, CancellationToken ct)
            => ToActionResult(await _wishlistService.RemoveFromWishlistAsync(GetEmailFromToken(), productId, ct));
    }
}