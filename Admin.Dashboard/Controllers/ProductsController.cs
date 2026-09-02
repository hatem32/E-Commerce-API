using AdminDashboard.Models.Products;
using AdminDashboard.Services;
using E_Commerce.Domain.Entities.Products;
using E_Commerce.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers
{
    public class ProductsController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly ProductImageStorageService _imageStorage;

        public ProductsController(StoreDbContext context, ProductImageStorageService imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductType)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductType)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductFormViewModel
            {
                Brands = await GetBrandsSelectListAsync(),
                Types = await GetTypesSelectListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            // A product needs an image from one of the two sources.
            if (model.ImageFile is null && string.IsNullOrWhiteSpace(model.PictureUrl))
            {
                ModelState.AddModelError(nameof(model.ImageFile), "Please upload an image or provide a picture URL.");
            }

            if (model.ImageFile is not null && !_imageStorage.IsValid(model.ImageFile, out var validationError))
            {
                ModelState.AddModelError(nameof(model.ImageFile), validationError!);
            }

            if (!ModelState.IsValid)
            {
                model.Brands = await GetBrandsSelectListAsync();
                model.Types = await GetTypesSelectListAsync();
                return View(model);
            }

            var pictureUrl = model.ImageFile is not null
                ? await _imageStorage.SaveAsync(model.ImageFile)
                : model.PictureUrl ?? string.Empty;

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                PictureUrl = pictureUrl,
                Price = model.Price,
                BrandId = model.BrandId,
                TypeId = model.TypeId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            var model = new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PictureUrl = product.PictureUrl,
                Price = product.Price,
                BrandId = product.BrandId,
                TypeId = product.TypeId,
                Brands = await GetBrandsSelectListAsync(),
                Types = await GetTypesSelectListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductFormViewModel model)
        {
            if (model.ImageFile is not null && !_imageStorage.IsValid(model.ImageFile, out var validationError))
            {
                ModelState.AddModelError(nameof(model.ImageFile), validationError!);
            }

            if (!ModelState.IsValid)
            {
                model.Brands = await GetBrandsSelectListAsync();
                model.Types = await GetTypesSelectListAsync();
                return View(model);
            }

            var product = await _context.Products.FindAsync(model.Id);

            if (product is null)
            {
                return NotFound();
            }

            if (model.ImageFile is not null)
            {
                // Replacing the image - upload the new one first, then clean up the old file.
                var newPictureUrl = await _imageStorage.SaveAsync(model.ImageFile);
                _imageStorage.TryDeleteExisting(product.PictureUrl);
                product.PictureUrl = newPictureUrl;
            }
            else if (!string.IsNullOrWhiteSpace(model.PictureUrl) && model.PictureUrl != product.PictureUrl)
            {
                // The Picture URL text field was edited manually instead of uploading a file.
                product.PictureUrl = model.PictureUrl;
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.BrandId = model.BrandId;
            product.TypeId = model.TypeId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductType)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product is not null)
            {
                _imageStorage.TryDeleteExisting(product.PictureUrl);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetBrandsSelectListAsync()
        {
            return await _context.ProductBrands
                .OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
                .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetTypesSelectListAsync()
        {
            return await _context.ProductTypes
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                .ToListAsync();
        }
    }
}