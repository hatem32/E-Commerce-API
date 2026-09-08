using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Application.DTOs.Authentications
{
    // Returned right after Register - deliberately has NO token. The account
    // exists but can't log in until the OTP is verified.
    public class RegisterResultDto
    {
        public string Email { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string Message { get; set; } = "Please check your email for a verification code.";
    }

    public class VerifyOtpDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = default!;
    }

    public class ResendOtpDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;
    }
}