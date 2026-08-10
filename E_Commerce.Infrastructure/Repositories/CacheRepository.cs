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
    }
}
