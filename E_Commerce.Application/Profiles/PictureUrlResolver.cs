using AutoMapper;
using E_Commerce.Application.DTOs.Products;
using E_Commerce.Domain.Entities.Products;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Profiles
{
    public class PictureUrlResolver(IOptions<UrlSettings> options) : IValueResolver<Product, ProductDto, string>
    {
        private readonly UrlSettings _urlSettings = options.Value;

        public string Resolve(Product source, ProductDto destination,
                               string destMember, ResolutionContext context)
        {
            var pictureUrl = source.PictureUrl;

            if (string.IsNullOrEmpty(pictureUrl))
                return string.Empty;

            if (pictureUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                pictureUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return pictureUrl;

            var baseUrl = _urlSettings.BaseUrl.TrimEnd('/');
            var path = pictureUrl.TrimStart('/');
            return $"{baseUrl}/Files/{path}";
        }
    }
    public class UrlSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
    }
}