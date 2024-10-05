using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace TestAPI.Attributes;

/// <summary>
/// if the current user with a idempontency key has not been processed, process and next function
/// different users can use the same idempotency key
/// ADD THIS TO CONTROLLERS [InterceptOutbound]
/// REMEMBER TO SET CACHE ON PROGRAM.CS
/// </summary>
public class InterceptOutbound(int expiryTime) : ActionFilterAttribute
{
    // Cache to store the processed keys via client id
    private static IMemoryCache? _cache;

    // Idempotency key header name
    private const string IdempotencyKeyHeader = "Idempotency-Key";

    // Expiry time for the cache to remove key, in days
    private int _expiryTime = expiryTime;

    // Set the Cache
    public static void SetCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    // Called async before the action is executed
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // get client id from the user claims
        string? client_id = context.HttpContext.User.FindFirst("client_id")?.Value;
        
        // Check for idempotency key in the request headers
        if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKey))
        {   
            // Return a 400 Bad Request response if the idempotency key is missing
            context.Result = new BadRequestObjectResult("Idempotency-Key header is missing.");
            return;
        }

        string true_idempotencyKey = $"{client_id}-{idempotencyKey}";
        // Get the client cache procssed keys
        Console.WriteLine(true_idempotencyKey);
        if (_cache!.TryGetValue(true_idempotencyKey, out _))
        {
            // if the key is in the cache
            // Return a 409 Conflict response if the idempotency key has already been processed
            context.Result = new ConflictObjectResult("This request has already been processed. Try Again");
            return;
        }

        // if key not processed yet then proceed with the next actions
        // Proceed with the action execution
        var executedContext = await next();

        // the executed context is the result of the action
        // so if the network failure, we can check here
        if (executedContext.Result is ObjectResult result)
        switch (result.StatusCode)
        {
            case 200 or 201:
                {
                    // Mark the key as processed in the cache
                    // Set the key in the cache to processed
                    // The key will be removed from the cache after the expiry time
                    // The default time span would be 10-5 days as Winston said
                    _cache.Set(true_idempotencyKey!, true, TimeSpan.FromDays(_expiryTime));
                }
                break;
            // Insert next status code here
            // case <code>: {}
            default:
                Console.WriteLine("Request has failed.");
                break;
        }


 
    }

}
