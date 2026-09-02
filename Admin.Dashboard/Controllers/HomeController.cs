using Admin.Dashboard.Models;
using AdminDashboard.Models;
using E_Commerce.Infrastructure.Data;
using E_Commerce.Infrastructure.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Admin.Dashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StoreDbContext _storeDbContext;
        private readonly StoreIdentityDbContext _identityDbContext;

        public HomeController(ILogger<HomeController> logger, StoreDbContext storeDbContext, StoreIdentityDbContext identityDbContext)
        {
            _logger = logger;
            _storeDbContext = storeDbContext;
            _identityDbContext = identityDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                UsersCount = await _identityDbContext.Users.CountAsync(),
                ProductsCount = await _storeDbContext.Products.CountAsync(),
                ProductBrandsCount = await _storeDbContext.ProductBrands.CountAsync(),
                ProductTypesCount = await _storeDbContext.ProductTypes.CountAsync(),
                OrdersCount = await _storeDbContext.Orders.CountAsync(),
                RecentProducts = await _storeDbContext.Products
                    .Include(p => p.ProductBrand)
                    .Include(p => p.ProductType)
                    .OrderByDescending(p => p.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
