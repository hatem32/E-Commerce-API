namespace E_Commerce.Application.Contracts
{
    public interface IOtpService
    {
        /// <summary>Generates a new 6-digit code, stores it (10 min expiry), and returns it to email.</summary>
        Task<string> GenerateAsync(string email, CancellationToken ct = default);

        /// <summary>Checks the code against what's stored for this email. Consumes it (one-time use) on success.</summary>
        Task<bool> ValidateAsync(string email, string code, CancellationToken ct = default);
    }
}