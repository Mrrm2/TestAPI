using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using TestAPI.IdempotencyLibrary.Attributes;
using TestAPI.IdempotencyLibrary.Interfaces;

namespace TestAPI.IdempotencyLibrary.Filters;

public class IdempotencyFilter : IAsyncActionFilter
{
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly ILogger<IdempotencyFilter> _logger;

    public IdempotencyFilter(IIdempotencyStore idempotencyStore, ILogger<IdempotencyFilter> logger)
    {
        _idempotencyStore = idempotencyStore;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
        var idempotencyAttribute = method?.GetCustomAttribute<IdempotencyAttribute>();

        if (idempotencyAttribute != null)
        {
            // Validate HTTP method
            if (!idempotencyAttribute.HttpMethods.Contains(context.HttpContext.Request.Method, StringComparer.OrdinalIgnoreCase))
            {
                // Proceed without idempotency if the method is not in the allowed list
                await next();
                return;
            }

            var request = context.HttpContext.Request;

            // Extract Idempotency-Key header
            string idempotencyKey = request.Headers["Idempotency-Key"]!;
            if (string.IsNullOrEmpty(idempotencyKey) && idempotencyAttribute.RejectRequestsWithoutKey)
            {
                context.Result = new BadRequestObjectResult("Idempotency-Key header is missing.");
                return;
            }

            // Validate and extract Client ID from access token (OAuth2)
            var user = context.HttpContext.User;
            if (!user.Identity!.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Uses the hardcoded client ID for local testing or extract it from token for production
            string clientId = user.FindFirst("client_id")?.Value ?? "test-client-id";
            if (string.IsNullOrEmpty(clientId))
            {
                context.Result = new UnauthorizedObjectResult("Client ID not found in access token.");
                return;
            }

            // Check if the idempotency key exists for this client
            var cachedResponse = await _idempotencyStore.GetResponseAsync(clientId, idempotencyKey);
            if (cachedResponse != null)
            {
                // Return the cached response
                context.Result = new OkObjectResult(cachedResponse);
                return;
            }

            // Proceed with the action execution
                var executedContext = await next();

                // After the action executes, store the response if the request was successful
                if (executedContext.Result is ObjectResult objectResult && (objectResult.StatusCode == 200 || objectResult.StatusCode == 201))
                {
                    await _idempotencyStore.SaveResponseAsync(clientId, idempotencyKey, objectResult.Value!, TimeSpan.FromHours(idempotencyAttribute.CacheExpiryInHours));
                }
            }
            else
            {
                // Proceed with the action execution if no IdempotencyAttribute is applied
                await next();
            }
    }
}

