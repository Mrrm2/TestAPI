using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

// Intercept Outbound Attribute
public class IdempotentAttribute : ActionFilterAttribute
{
    private static IMemoryCache _cache;

    public static void SetMemoryCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        
        // Check for idempotency key in the request headers
        if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
        {
            context.Result = new BadRequestObjectResult("Idempotency-Key header is missing.");
            return;
        }

        // Check if the idempotency key has already been processed
        if (_cache.TryGetValue(idempotencyKey.ToString(), out _))
        {
            context.Result = new ConflictObjectResult("This request has already been processed.");
            return;
        }

        // Proceed with the action execution
        base.OnActionExecuting(context);
    }

    public static void MarkAsProcessed(string idempotencyKey)
    {
        // Mark the key as processed in the cache
        // can change add an expiration in the cache to prevent memory leaks
        _cache.Set(idempotencyKey, true);
    }
}
