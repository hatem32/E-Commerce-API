using E_Commerce.Domain.Contracts;

namespace E_Commerce.API.Extensions
{
    internal static class WebApplicationExtensions
    {
        public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredKeyedService<IDataSeeder>("Catalog");
            var IdentitySeeder = scope.ServiceProvider.GetRequiredKeyedService<IDataSeeder>("Identity");
            await seeder.SeedDataAsync();
            await IdentitySeeder.SeedDataAsync();
            return app;
        }
    }
}
