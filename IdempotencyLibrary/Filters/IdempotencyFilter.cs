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
                await next();
                return;
            }

            // Extract Idempotency-Key header
            if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) && idempotencyAttribute.RejectRequestsWithoutKey)
            {
                context.Result = new BadRequestObjectResult("Idempotency-Key header is missing.");
                return;
            }

            // Extract client ID directly from JWT token
            var clientId = context.HttpContext.User.FindFirst("client_id")?.Value;
            if (string.IsNullOrEmpty(clientId))
            {
                context.Result = new UnauthorizedObjectResult("Client ID not found in the JWT token.");
                return;
            }

            // Check if a cached response exists
            var cachedResponse = await _idempotencyStore.GetResponseAsync(clientId, idempotencyKey);
            if (cachedResponse != null)
            {
                context.Result = new OkObjectResult(cachedResponse);
                return;
            }

            await next();
        }
        else
        {
            await next();
        }
    }
}
