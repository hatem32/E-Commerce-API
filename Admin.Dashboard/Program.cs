using AdminDashboard.Options;
using AdminDashboard.Services;
using E_Commerce.Application.Profiles;
using E_Commerce.Infrastructure.Data;
using E_Commerce.Infrastructure.Identity.Data;
using E_Commerce.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Admin.Dashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<StoreDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddDbContext<StoreIdentityDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
            });


            //add Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                            .AddEntityFrameworkStores<StoreIdentityDbContext>()
                            .AddDefaultTokenProviders();

            // Same UrlSettings used by the API, so the dashboard can build full image URLs
            // (e.g. https://localhost:7227/Files/images/products/x.jpeg) from the relative
            // PictureUrl path stored in the database.
            builder.Services.Configure<UrlSettings>(builder.Configuration.GetSection("UrlSettings"));

            // Where uploaded product images get physically saved - the API's Files/images/products folder.
            builder.Services.Configure<ProductImagesSettings>(builder.Configuration.GetSection("ProductImages"));
            builder.Services.AddScoped<ProductImageStorageService>();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                 name: "default",
                 pattern: "{controller=Admin}/{action=Login}/{id?}");

            app.Run();
        }
    }
}