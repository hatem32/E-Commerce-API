using AutoMapper;
using E_Commerce.Application.DTOs.Wishlists;
using E_Commerce.Domain.Entities.Wishlists;
using Microsoft.Extensions.Options;
using System;

namespace E_Commerce.Application.Profiles
{
    public class WishlistProfile : Profile
    {
        public WishlistProfile()
        {
            CreateMap<WishlistItem, WishlistItemDto>()
                .ForMember(dst => dst.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dst => dst.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dst => dst.ProductBrand, opt => opt.MapFrom(src => src.Product.ProductBrand.Name))
                .ForMember(dst => dst.ProductType, opt => opt.MapFrom(src => src.Product.ProductType.Name))
                .ForMember(dst => dst.PictureUrl, opt => opt.MapFrom<WishlistPictureUrlResolver>());
        }
    }

    public class WishlistPictureUrlResolver : IValueResolver<WishlistItem, WishlistItemDto, string>
    {
        private readonly UrlSettings _urlSettings;

        public WishlistPictureUrlResolver(IOptions<UrlSettings> options)
        {
            _urlSettings = options.Value;
        }

        public string Resolve(WishlistItem source, WishlistItemDto destination, string destMember, ResolutionContext context)
        {
            var pictureUrl = source.Product.PictureUrl;

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
}