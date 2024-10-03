// using System.Text.Json;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;
// using Microsoft.Extensions.Caching.Memory;
// using Microsoft.Extensions.DependencyInjection;

// namespace IdempotentUtils;

// [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
// public class IdempotentAttribute : ActionFilterAttribute
// {
//   // static one way to share cache across all instances of the attribute
//   public readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

//   // other way is through dependency injection in the constructor
//   // Get the shared IMemoryCache instance from the dependency injection container
//   // private IMemoryCache? _cache;
//   // program.cs > services.AddMemoryCache();

//   private readonly int _expirationInSeconds;
//   private readonly string _cacheKeyPrefix;
//   public IdempotentAttribute(string cacheKeyPrefix, int expirationInSeconds = 500)
//     {
//         _cacheKeyPrefix = cacheKeyPrefix;
//         _expirationInSeconds = expirationInSeconds;
//     }

//     public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
//   {
//     // _cache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
//     var request = context.HttpContext.Request;
//     var idempotencyKey = request.Headers["Idempotency-Key"];

//     if (string.IsNullOrEmpty(idempotencyKey))
//     {
//       context.Result = new BadRequestObjectResult("Idempotency-Key header is missing");
//       return;
//     }

//     var cacheKey = $"{_cacheKeyPrefix}-{idempotencyKey}"; 
//     Console.WriteLine($"Cache key: {cacheKey}");

//     if (_cache!.TryGetValue(cacheKey, out var cachedResponse))
//     {
//       var jsonResponse = JsonSerializer.Serialize(cachedResponse);

//       context.Result = new ContentResult {
//         Content = jsonResponse,
//         ContentType = "application/json",
//         StatusCode = 200
//       };
//       return;
//     }

//     var responseContext = await next();

//     if (responseContext.Result is ObjectResult objectResult)
//     {
//       Console.WriteLine($"Setting cache for key: {cacheKey}");
//       var cacheEntryOptions = new MemoryCacheEntryOptions()
//         .SetAbsoluteExpiration(TimeSpan.FromSeconds(_expirationInSeconds));

//       _cache.Set(cacheKey, objectResult.Value, cacheEntryOptions);
//     }
//   }
// }
