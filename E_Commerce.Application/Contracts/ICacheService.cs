using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Contracts
{
    public interface ICacheService
    {
        Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default);
        Task SetAsync(string cacheKey, object cacheValue, TimeSpan timeToLive, CancellationToken cancellationToken = default);
    }
}
