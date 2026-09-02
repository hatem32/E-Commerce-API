using AdminDashboard.Models.Products;
using E_Commerce.Domain.Entities.Products;
using E_Commerce.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers
{
    public class ProductTypesController : Controller
    {
        private readonly StoreDbContext _context;

        public ProductTypesController(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var types = await _context.ProductTypes.OrderBy(b => b.Name).ToListAsync();
            return View(types);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductNameViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.ProductTypes.AnyAsync(b => b.Name == model.Name);

                if (!exists)
                {
                    _context.ProductTypes.Add(new ProductType { Name = model.Name });
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("Name", "This type already exists");
            }

            return View(nameof(Index), await _context.ProductTypes.OrderBy(b => b.Name).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var type = await _context.ProductTypes.FindAsync(id);

            if (type is null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new UpdatedProductNameViewModel { Id = type.Id, Name = type.Name });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdatedProductNameViewModel model)
        {
            if (ModelState.IsValid)
            {
                var type = await _context.ProductTypes.FindAsync(model.Id);

                if (type is not null)
                {
                    type.Name = model.Name;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var type = await _context.ProductTypes.FindAsync(id);

            if (type is not null)
            {
                var inUse = await _context.Products.AnyAsync(p => p.TypeId == id);

                if (inUse)
                {
                    TempData["Error"] = "This type can't be deleted because it's used by one or more products.";
                    return RedirectToAction(nameof(Index));
                }

                _context.ProductTypes.Remove(type);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
