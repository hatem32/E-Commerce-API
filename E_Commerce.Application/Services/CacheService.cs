using E_Commerce.Application.Contracts;
using E_Commerce.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace E_Commerce.Application.Services
{
    public class CacheService : ICacheService
    {
        private readonly ICacheRepository _cacheRepository;

        public CacheService(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }
        public Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
            => _cacheRepository.GetAsync(cacheKey, cancellationToken);

        public Task SetAsync(string cacheKey, object cacheValue, TimeSpan timeToLive, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(cacheValue, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            return _cacheRepository.SetAsync(cacheKey, json, timeToLive, cancellationToken);
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
            => _cacheRepository.RemoveByPrefixAsync(prefix, cancellationToken);
    }
}