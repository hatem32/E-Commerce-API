using E_Commerce.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace E_Commerce.API.Attributes
{
    public class RedisCacheAttribute : ActionFilterAttribute
    {
        private readonly int _durationInSec;

        public RedisCacheAttribute(int durationInSec = 90)
        {
            _durationInSec = durationInSec;
        }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get Cache Service from Container [Not Injection Direct In Constructor]
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();

            var cacheKey = CreateCacheKey(context.HttpContext.Request);
            var cached = await cacheService.GetAsync(cacheKey);

            // If Data Exists in Cache -> Get From Cache and Skip Endpoint 
            if (!string.IsNullOrEmpty(cached))
            {
                context.Result = new ContentResult
                {
                    Content = cached,
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status200OK
                };
                return;
            }

            // If Not Get Data -> Execute EndPoint and Store Result in Cache if Result Is Ok  
            var executed = await next.Invoke();
            if (executed.Result is OkObjectResult { Value: not null } ok)
                await cacheService.SetAsync(cacheKey, ok.Value, TimeSpan.FromSeconds(_durationInSec));
        }

        private static string CreateCacheKey(HttpRequest request)
        {
            var key = new StringBuilder();
            key.Append(request.Path).Append('?');
            foreach (var (k, v) in request.Query.OrderBy(q => q.Key))
                key.Append(k).Append('=').Append(v).Append('&');
            return key.ToString();
        }
    }
}
