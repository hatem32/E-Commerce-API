using E_Commerce.API.Attributes;
using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API.Controllers
{

    public class ProductsController : ApiBaseController
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;

        public ProductsController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }
        // Get all products
        [HttpGet]
        [RedisCache(100)]
        public async Task<ActionResult<PaginatedResult<ProductDto>>> GetAllProducts([FromQuery] ProductQueryParams queryParams, CancellationToken ct)
        {
            var result = await _productService.GetAllProductsAsync(queryParams, ct);
            return ToActionResult(result);
        }



        // Get product by id
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProduct(int id, CancellationToken ct)
        {
            var result = await _productService.GetProductByIdAsync(id, ct);
            return ToActionResult(result);
        }



        // Get all brands
        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<BrandDto>>> GetAllBrands(CancellationToken ct)
        {
            var result = await _productService.GetAllBrandsAsync(ct);
            return ToActionResult(result);
        }



        // Get all types
        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<TypeDto>>> GetAllTypes(CancellationToken ct)
        {
            var result = await _productService.GetAllTypesAsync(ct);
            return ToActionResult(result);
        }

        // ==================== Admin: Products ====================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductFormDto dto, CancellationToken ct)
            => ToActionResult(await _productService.CreateProductAsync(dto, ct));

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, ProductFormDto dto, CancellationToken ct)
            => ToActionResult(await _productService.UpdateProductAsync(id, dto, ct));

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> DeleteProduct(int id, CancellationToken ct)
            => ToActionResult(await _productService.DeleteProductAsync(id, ct));

        // Uploads a product image to /Files/images/products and returns its relative path
        // (e.g. "images/products/xxxx.jpg") - use that as ProductFormDto.PictureUrl.
        [Authorize(Roles = "Admin")]
        [HttpPost("images")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadProductImage(IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file was uploaded.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
                return BadRequest($"Unsupported image type '{extension}'. Allowed types: {string.Join(", ", AllowedImageExtensions)}.");

            if (file.Length > MaxImageSizeBytes)
                return BadRequest("Image is too large. Maximum allowed size is 5 MB.");

            var folder = Path.Combine(_env.ContentRootPath, "Files", "images", "products");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            return Ok(new { pictureUrl = $"images/products/{fileName}" });
        }

        // ==================== Admin: Brands ====================

        [Authorize(Roles = "Admin")]
        [HttpPost("brands")]
        [ProducesResponseType(typeof(BrandDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<BrandDto>> CreateBrand(NameFormDto dto, CancellationToken ct)
            => ToActionResult(await _productService.CreateBrandAsync(dto, ct));

        [Authorize(Roles = "Admin")]
        [HttpPut("brands/{id:int}")]
        [ProducesResponseType(typeof(BrandDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<BrandDto>> UpdateBrand(int id, NameFormDto dto, CancellationToken ct)
            => ToActionResult(await _productService.UpdateBrandAsync(id, dto, ct));

        [Authorize(Roles = "Admin")]
        [HttpDelete("brands/{id:int}")]
        public async Task<ActionResult<bool>> DeleteBrand(int id, CancellationToken ct)
            => ToActionResult(await _productService.DeleteBrandAsync(id, ct));

        // ==================== Admin: Types ====================

        [Authorize(Roles = "Admin")]
        [HttpPost("types")]
        [ProducesResponseType(typeof(TypeDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TypeDto>> CreateType(NameFormDto dto, CancellationToken ct)
            => ToActionResult(await _productService.CreateTypeAsync(dto, ct));

        [Authorize(Roles = "Admin")]
        [HttpPut("types/{id:int}")]
        [ProducesResponseType(typeof(TypeDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TypeDto>> UpdateType(int id, NameFormDto dto, CancellationToken ct)
            => ToActionResult(await _productService.UpdateTypeAsync(id, dto, ct));

        [Authorize(Roles = "Admin")]
        [HttpDelete("types/{id:int}")]
        public async Task<ActionResult<bool>> DeleteType(int id, CancellationToken ct)
            => ToActionResult(await _productService.DeleteTypeAsync(id, ct));
    }
}