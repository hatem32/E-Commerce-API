using E_Commerce.Application.Common;
using E_Commerce.Application.DTOs.Authentications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Contracts
{
    public interface IIdentityService
    {
        Task<Result<IdentityUserResult>> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<bool>> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<Result<IdentityUserResult>> CreateUserAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<string>>> GetRolesAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<AddressDto>> GetAddressByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<AddressDto>> UpSertAddressAsync(string email, AddressDto addressDto, CancellationToken cancellationToken = default);
        Task<Result<bool>> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<bool>> ConfirmEmailAsync(string email, CancellationToken cancellationToken = default);

    }
}