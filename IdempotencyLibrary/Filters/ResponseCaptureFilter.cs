using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using TestAPI.IdempotencyLibrary.Attributes;
using TestAPI.IdempotencyLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace TestAPI.IdempotencyLibrary.Filters
{
    public class ResponseCaptureFilter : IAsyncActionFilter
    {
        private readonly IIdempotencyStore _idempotencyStore;
        private readonly ILogger<ResponseCaptureFilter> _logger;

        public ResponseCaptureFilter(IIdempotencyStore idempotencyStore, ILogger<ResponseCaptureFilter> logger)
        {
            _idempotencyStore = idempotencyStore;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
            var responseCaptureAttribute = method?.GetCustomAttribute<ResponseCaptureAttribute>();

            if (responseCaptureAttribute != null)
            {
                var resultContext = await next();

                // Only cache responses for specified status codes
                if (resultContext.Result is ObjectResult objectResult &&
                    responseCaptureAttribute.StatusCodesToCapture.Contains(objectResult.StatusCode ?? 200))
                {
                    var request = context.HttpContext.Request;

                    // Extract Idempotency-Key header
                    if (request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) &&
                        resultContext.HttpContext.User.Identity?.IsAuthenticated == true)
                    {
                        string clientId = resultContext.HttpContext.User.FindFirst("client_id")?.Value ?? "test-client-id"; // For local testing

                        // Add metadata if specified
                        var responseData = objectResult.Value;
                        if (responseCaptureAttribute.IncludeMetadata)
                        {
                            responseData = new
                            {
                                Response = objectResult.Value,
                                Metadata = new
                                {
                                    Timestamp = DateTime.UtcNow,
                                    RequestPath = request.Path,
                                    ClientId = clientId
                                }
                            };
                        }

                        // Cache the response, asynchronously if required
                        if (responseCaptureAttribute.OnlyCacheSuccessfulResponses && objectResult.StatusCode >= 200 && objectResult.StatusCode < 300)
                        {
                            if (responseCaptureAttribute.CacheAsynchronously)
                            {
                                _ = _idempotencyStore.SaveResponseAsync(clientId, idempotencyKey, responseData, TimeSpan.FromHours(responseCaptureAttribute.CacheExpiryInHours));
                            }
                            else
                            {
                                await _idempotencyStore.SaveResponseAsync(clientId, idempotencyKey, responseData, TimeSpan.FromHours(responseCaptureAttribute.CacheExpiryInHours));
                            }

                            _logger.LogInformation("Captured response for client {ClientId} with key {IdempotencyKey}.", clientId, idempotencyKey);
                        }
                    }
                }
            }
            else
            {
                await next();
            }
        }
    }
}
