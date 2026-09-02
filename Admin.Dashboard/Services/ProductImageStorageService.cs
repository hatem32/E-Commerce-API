using AdminDashboard.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AdminDashboard.Services
{
    public class ProductImageStorageService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        private readonly ProductImagesSettings _settings;
        private readonly IWebHostEnvironment _env;

        public ProductImageStorageService(IOptions<ProductImagesSettings> settings, IWebHostEnvironment env)
        {
            _settings = settings.Value;
            _env = env;
        }

        public bool IsValid(IFormFile file, out string? error)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                error = $"Unsupported image type '{extension}'. Allowed types: {string.Join(", ", AllowedExtensions)}.";
                return false;
            }

            if (file.Length > MaxFileSizeBytes)
            {
                error = "Image is too large. Maximum allowed size is 5 MB.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Saves the uploaded file to the API's Files/images/products folder and
        /// returns the relative path (e.g. "images/products/xxxxx.jpg") to store in the database.
        /// </summary>
        public async Task<string> SaveAsync(IFormFile file)
        {
            var physicalFolder = ResolvePhysicalFolder();
            Directory.CreateDirectory(physicalFolder);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(physicalFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePrefix = _settings.RelativePath.Trim('/');
            return $"{relativePrefix}/{fileName}";
        }

        /// <summary>
        /// Deletes a previously uploaded image from disk. Safe to call with any PictureUrl value -
        /// full external URLs are ignored so we never try (or need) to delete anything off this server.
        /// </summary>
        public void TryDeleteExisting(string? pictureUrl)
        {
            if (string.IsNullOrWhiteSpace(pictureUrl))
            {
                return;
            }

            if (pictureUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                pictureUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var fileName = Path.GetFileName(pictureUrl);
            var physicalFolder = ResolvePhysicalFolder();
            var fullPath = Path.Combine(physicalFolder, fileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (IOException)
                {
                    // File in use or already gone - not worth failing the request over.
                }
            }
        }

        private string ResolvePhysicalFolder()
        {
            return Path.IsPathRooted(_settings.PhysicalPath)
                ? _settings.PhysicalPath
                : Path.GetFullPath(Path.Combine(_env.ContentRootPath, _settings.PhysicalPath));
        }
    }
}