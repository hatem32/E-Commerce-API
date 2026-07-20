using E_Commerce.Domain.Contracts;
using E_Commerce.Domain.Entities.Products;
using E_Commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace E_Commerce.Infrastructure.DataSeeding
{
    internal class CatalogDataSeeder(StoreDbContext dbContext, ILogger<CatalogDataSeeder> logger) : IDataSeeder
    {
        public async Task SeedDataAsync(CancellationToken ct = default)
        {
            try
            {
                var PendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(ct);
                if (PendingMigrations.Any())
                {
                    await dbContext.Database.MigrateAsync(ct);
                }

                // seeding
                // path
                // "D:\my programs\Route (.Net)\9- API\E_Commerce\E_Commerce.API\bin\Debug\net8.0\DataSeed\products.json"

                var SeedRoot = Path.Combine(AppContext.BaseDirectory, "DataSeed");


                await SeedIfEmptyAsync<ProductBrand>(SeedRoot, "brands.json", ct);
                await SeedIfEmptyAsync<ProductType>(SeedRoot, "types.json", ct);
                await SeedIfEmptyAsync<Product>(SeedRoot, "products.json", ct);

                await dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Catalog data seeding failed.");
                throw;
            }
        }


        private async Task SeedIfEmptyAsync<T>(string root, string fileName, CancellationToken ct) where T : class
        {
            if (await dbContext.Set<T>().AnyAsync(ct)) return;

            var path = Path.Combine(root, fileName);
            if (!File.Exists(path))
            {
                logger.LogWarning("Seed file not found: {Path}", path);
                return;
            }

            await using var stream = File.OpenRead(path);
            var items = await JsonSerializer.DeserializeAsync<List<T>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                ct);

            if (items is { Count: > 0 })
                await dbContext.Set<T>().AddRangeAsync(items, ct);
        }


    }
}
