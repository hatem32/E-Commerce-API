using E_Commerce.Application.Contracts;

namespace E_Commerce.Application.Services
{
    internal class OtpService : IOtpService
    {
        private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);
        private readonly ICacheService _cacheService;

        public OtpService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<string> GenerateAsync(string email, CancellationToken ct = default)
        {
            var code = Random.Shared.Next(0, 1_000_000).ToString("D6");
            await _cacheService.SetAsync(CacheKey(email), code, Ttl, ct);
            return code;
        }

        public async Task<bool> ValidateAsync(string email, string code, CancellationToken ct = default)
        {
            var stored = await _cacheService.GetAsync(CacheKey(email), ct);

            // Stored values are JSON-serialized by CacheService.SetAsync, so a plain string
            // comes back wrapped in quotes (e.g. "\"123456\"") - strip them before comparing.
            var normalizedStored = stored?.Trim('"');

            if (string.IsNullOrEmpty(normalizedStored) || normalizedStored != code)
                return false;

            await _cacheService.RemoveByPrefixAsync(CacheKey(email), ct);
            return true;
        }

        private static string CacheKey(string email) => $"otp:{email.Trim().ToLowerInvariant()}";
    }
}