using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Authentications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public AuthenticationService(IIdentityService identityService, ITokenService tokenService,
            IOtpService otpService, IEmailService emailService)
        {
            _identityService = identityService;
            _tokenService = tokenService;
            _otpService = otpService;
            _emailService = emailService;
        }

        public async Task<Result<bool>> CheckEmailAsync(string email, CancellationToken cancellationToken = default)
                => await _identityService.EmailExistsAsync(email, cancellationToken);

        public async Task<Result<UserDto>> GetCurrentUserAsync(string email, CancellationToken cancellationToken = default)
        {
            var result = await _identityService.FindByEmailAsync(email, cancellationToken);

            if (!result.IsSuccess)
                return Result<UserDto>.Fail(result.Errors);

            var user = result.data;
            var rolesResult = await _identityService.GetRolesAsync(email, cancellationToken);
            if (!rolesResult.IsSuccess)
                return Result<UserDto>.Fail(rolesResult.Errors);
            var roles = rolesResult.data;
            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, roles);

            return new UserDto { DisplayName = user.DisplayName, Email = user.Email, Token = token };

        }

        public async Task<Result<AddressDto>> GetUserAddressAsync(string email, CancellationToken cancellationToken = default)
        {
            var result = await _identityService.GetAddressByEmailAsync(email, cancellationToken);
            if (!result.IsSuccess)
                return Result<AddressDto>.Fail(result.Errors);
            return result.data;
        }

        public async Task<Result<UserDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            // Get User By Email 
            var userResult = await _identityService.FindByEmailAsync(loginDto.Email, cancellationToken);
            if (!userResult.IsSuccess)
                return Result<UserDto>.Fail(userResult.Errors);

            // Check Password 
            var passwordResult = await _identityService.CheckPasswordAsync(loginDto.Email, loginDto.Password, cancellationToken);
            if (!passwordResult.IsSuccess)
                return Result<UserDto>.Fail(Error.Unauthorized("Invalid Email or Password"));

            var user = userResult.data;

            // Registered but never verified the OTP sent to their email.
            if (!user.EmailConfirmed)
                return Result<UserDto>.Fail(Error.Unauthorized("Please verify your email before logging in."));

            var rolesResult = await _identityService.GetRolesAsync(loginDto.Email, cancellationToken);
            if (!rolesResult.IsSuccess) return Result<UserDto>.Fail(rolesResult.Errors);

            var roles = rolesResult.data;
            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, roles);

            return new UserDto
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                Token = token
            };
        }

        public async Task<Result<RegisterResultDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
        {
            var result = await _identityService.CreateUserAsync(registerDto, cancellationToken);
            if (!result.IsSuccess || result.data is null)
                return Result<RegisterResultDto>.Fail(result.Errors);

            var user = result.data;

            await SendOtpEmailAsync(user.Email!, user.DisplayName, cancellationToken);

            return new RegisterResultDto { Email = user.Email!, DisplayName = user.DisplayName };
        }

        public async Task<Result<UserDto>> VerifyOtpAsync(VerifyOtpDto dto, CancellationToken cancellationToken = default)
        {
            var isValid = await _otpService.ValidateAsync(dto.Email, dto.Otp, cancellationToken);
            if (!isValid)
                return Result<UserDto>.Fail(Error.Validation("Otp.Invalid", "That code is invalid or has expired."));

            var confirmResult = await _identityService.ConfirmEmailAsync(dto.Email, cancellationToken);
            if (!confirmResult.IsSuccess)
                return Result<UserDto>.Fail(confirmResult.Errors);

            var userResult = await _identityService.FindByEmailAsync(dto.Email, cancellationToken);
            if (!userResult.IsSuccess)
                return Result<UserDto>.Fail(userResult.Errors);

            var rolesResult = await _identityService.GetRolesAsync(dto.Email, cancellationToken);
            if (!rolesResult.IsSuccess) return Result<UserDto>.Fail(rolesResult.Errors);

            var user = userResult.data;
            var roles = rolesResult.data;
            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, roles);

            return new UserDto
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                Token = token
            };
        }

        public async Task<Result<bool>> ResendOtpAsync(string email, CancellationToken cancellationToken = default)
        {
            var userResult = await _identityService.FindByEmailAsync(email, cancellationToken);
            if (!userResult.IsSuccess)
                return Result<bool>.Fail(userResult.Errors);

            if (userResult.data.EmailConfirmed)
                return Result<bool>.Fail(Error.Validation("Email.AlreadyVerified", "This account is already verified - please log in."));

            await SendOtpEmailAsync(userResult.data.Email!, userResult.data.DisplayName, cancellationToken);
            return true;
        }

        public async Task<Result<AddressDto>> UpdateUserAddressAsync(AddressDto addressDto, string email, CancellationToken cancellationToken = default)
                => await _identityService.UpSertAddressAsync(email, addressDto, cancellationToken);

        private async Task SendOtpEmailAsync(string email, string displayName, CancellationToken cancellationToken)
        {
            var otp = await _otpService.GenerateAsync(email, cancellationToken);

            var body = $"""
                <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
                    <h2 style="color:#1e3a8a;">Verify your email</h2>
                    <p>Hi {System.Net.WebUtility.HtmlEncode(displayName)},</p>
                    <p>Use the code below to verify your E-Shop account. It expires in 10 minutes.</p>
                    <div style="font-size: 32px; font-weight: bold; letter-spacing: 8px; background:#f3f4f6; padding: 16px 24px; text-align:center; border-radius: 8px; margin: 20px 0;">
                        {otp}
                    </div>
                    <p style="color:#6b7280; font-size: 0.9em;">If you didn't create this account, you can safely ignore this email.</p>
                </div>
                """;

            await _emailService.SendAsync(email, "Your E-Shop verification code", body, ct: cancellationToken);
        }
    }
}