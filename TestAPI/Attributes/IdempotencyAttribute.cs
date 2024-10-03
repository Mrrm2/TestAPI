using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TestAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IdempotencyAttribute : Attribute, IAsyncActionFilter
    {
        private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Retrieve the Idempotency-Key header from the request
            var idempotencyKey = context.HttpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();

            // If the key is not present, return a Bad Request response
            if (string.IsNullOrEmpty(idempotencyKey))
            {
                context.Result = new BadRequestObjectResult("Idempotency-Key header is required.");
                return;
            }

            // Check if a response for this idempotency key is already cached
            if (Cache.TryGetValue(idempotencyKey, out var cachedResponse))
            {
                // Return the cached response with a custom message
                var response = new
                {
                    Message = "This order has already been placed.",
                    CachedResponse = cachedResponse
                };
                context.Result = new OkObjectResult(response);
                return;
            }

            // Execute the action and capture the result
            var resultContext = await next();

            // If the result is successful, cache the response with the idempotency key
            if (resultContext.Result is OkObjectResult okResult)
            {
                Cache.Set(idempotencyKey, okResult.Value, TimeSpan.FromMinutes(5)); // Cache for 5 minutes
            }
        }
    }
}
