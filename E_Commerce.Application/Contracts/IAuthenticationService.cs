using E_Commerce.Application.Common;
using E_Commerce.Application.DTOs.Authentications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Contracts
{
    public interface IAuthenticationService
    {
        Task<Result<UserDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
        Task<Result<RegisterResultDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
        Task<Result<UserDto>> VerifyOtpAsync(VerifyOtpDto dto, CancellationToken cancellationToken = default);
        Task<Result<bool>> ResendOtpAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<bool>> CheckEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<AddressDto>> GetUserAddressAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<AddressDto>> UpdateUserAddressAsync(AddressDto addressDto, string email, CancellationToken cancellationToken = default);
        Task<Result<UserDto>> GetCurrentUserAsync(string email, CancellationToken cancellationToken = default);
    }
}