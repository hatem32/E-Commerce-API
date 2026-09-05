using E_Commerce.Domain.Contracts;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Infrastructure.Repositories
{
    internal class CacheRepository : ICacheRepository
    {
        private readonly IDatabase _database;
        public CacheRepository(IConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
        }
        public async Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            var value = await _database.StringGetAsync(cacheKey);
            return value.IsNullOrEmpty ? null : value.ToString();
        }

        public Task SetAsync(string cacheKey, string cacheValue, TimeSpan timeToLive, CancellationToken cancellationToken = default)
            => _database.StringSetAsync(cacheKey, cacheValue, timeToLive);

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var endpoints = _database.Multiplexer.GetEndPoints();
            if (endpoints.Length == 0)
                return;

            var server = _database.Multiplexer.GetServer(endpoints[0]);

            // Cache keys look like "/api/products?pageIndex=1&pageSize=8&..." - match every
            // variation (different paging/sorting/filters) that starts with this path.
            await foreach (var key in server.KeysAsync(pattern: $"{prefix}*"))
            {
                await _database.KeyDeleteAsync(key);
            }
        }
    }
}