using System;

namespace TestAPI.IdempotencyLibrary.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class IdempotencyAttribute : Attribute
{
    /// <summary>
    /// Determines the HTTP methods for which idempotency should be enforced.
    /// </summary>
    public string[] HttpMethods { get; set; } = { "POST", "PUT" };

    /// <summary>
    /// Specifies the cache expiry duration in hours for the idempotency key.
    /// Defaults to 24 hours but can be set by the implementing developer.
    /// </summary>
    public int CacheExpiryInHours { get; set; }

    /// <summary>
    /// Indicates whether requests without a valid Idempotency-Key should be rejected.
    /// Defaults to true.
    /// </summary>
    public bool RejectRequestsWithoutKey { get; set; } = true;

    /// <summary>
    /// Specifies the default retry count for operations in case of failure.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Specifies if caching should be performed using in-memory cache only.
    /// </summary>
    public bool UseInMemoryCache { get; set; } = true;

    /// <summary>
    /// Constructor to allow the user to specify the cache expiry duration.
    /// </summary>
    /// <param name="cacheExpiryInHours">The expiry time for the idempotency cache in hours.</param>
    public IdempotencyAttribute(int cacheExpiryInHours = 24)
    {
        CacheExpiryInHours = cacheExpiryInHours;
    }
}
