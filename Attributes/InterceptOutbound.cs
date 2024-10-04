using System;
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
public class InterceptOutbound : ActionFilterAttribute
{
    // Cache to store the processed keys via client id
    private static IMemoryCache? _cache;

    // Idempotency key header name
    private const string IdempotencyKeyHeader = "Idempotency-Key";

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

        // Get the client cache procssed keys
        if (_cache!.TryGetValue(client_id!, out var keys))
            // if the key is in the cache
            // convert to list
            if (keys is List<string> keyList)
                // if the current user's keylist contains the idempotency key
                if (keyList.Contains(idempotencyKey.ToString()))
                {
                    // Return a 409 Conflict response if the idempotency key has already been processed
                    context.Result = new ConflictObjectResult("This request has already been processed. Try Again");
                    return;
                }

        // if key not processed yet then proceed with the next actions
        // Proceed with the action execution
        var executedContext = await next();

        // Mark the key as processed in the cache
        if (executedContext.Result is ObjectResult result && result.StatusCode == 200)
        {
            Console.WriteLine("Request has been processed successfully.");
            
            // if there is no list for user
            if (keys == null)
            {
                // create a list
                _cache.Set(client_id!, new List<string> { idempotencyKey.ToString() });
                return;
            }

            // append the idempotency key to the list of processed keys
            List<string>? myKeys = keys as List<string>;
            myKeys!.Append(idempotencyKey.ToString());
            _cache.Set(client_id!, myKeys);
        }
    }

}
