using E_Commerce.Application.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Infrastructure.Identity.Services
{
    public class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
    {
        private readonly JwtSettings _settings = jwtOptions.Value;

        public string CreateToken(string userId, string email, string userName, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, userName)
        };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
                throw new InvalidOperationException("JWT SecretKey is missing");

            if (_settings.SecretKey.Length < 32)
                throw new InvalidOperationException("JWT SecretKey is too short");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,

                expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }


    public class JwtSettings
    {
        public string SecretKey { get; init; } = default!;
        public string Issuer { get; init; } = default!;
        public string Audience { get; init; } = default!;
        public int ExpirationMinutes { get; init; } = 60;
    }
}
