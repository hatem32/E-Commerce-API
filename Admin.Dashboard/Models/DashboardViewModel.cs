using E_Commerce.Domain.Entities.Products;

namespace AdminDashboard.Models
{
    public class DashboardViewModel
    {
        public int UsersCount { get; set; }
        public int ProductsCount { get; set; }
        public int ProductBrandsCount { get; set; }
        public int ProductTypesCount { get; set; }
        public int OrdersCount { get; set; }

        public List<Product> RecentProducts { get; set; } = new();
    }
}
