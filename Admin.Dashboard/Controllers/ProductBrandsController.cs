using AdminDashboard.Models.Products;
using E_Commerce.Domain.Entities.Products;
using E_Commerce.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers
{
    public class ProductBrandsController : Controller
    {
        private readonly StoreDbContext _context;

        public ProductBrandsController(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _context.ProductBrands.OrderBy(b => b.Name).ToListAsync();
            return View(brands);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductNameViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.ProductBrands.AnyAsync(b => b.Name == model.Name);

                if (!exists)
                {
                    _context.ProductBrands.Add(new ProductBrand { Name = model.Name });
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("Name", "This brand already exists");
            }

            return View(nameof(Index), await _context.ProductBrands.OrderBy(b => b.Name).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _context.ProductBrands.FindAsync(id);

            if (brand is null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new UpdatedProductNameViewModel { Id = brand.Id, Name = brand.Name });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdatedProductNameViewModel model)
        {
            if (ModelState.IsValid)
            {
                var brand = await _context.ProductBrands.FindAsync(model.Id);

                if (brand is not null)
                {
                    brand.Name = model.Name;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.ProductBrands.FindAsync(id);

            if (brand is not null)
            {
                var inUse = await _context.Products.AnyAsync(p => p.BrandId == id);

                if (inUse)
                {
                    TempData["Error"] = "This brand can't be deleted because it's used by one or more products.";
                    return RedirectToAction(nameof(Index));
                }

                _context.ProductBrands.Remove(brand);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
