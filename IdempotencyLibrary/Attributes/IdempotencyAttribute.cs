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
    /// Defaults to 24 hours.
    /// </summary>
    public int CacheExpiryInHours { get; set; } = 24;

    /// <summary>
    /// Indicates whether requests without a valid Idempotency-Key should be rejected.
    /// Defaults to true.
    /// </summary>
    public bool RejectRequestsWithoutKey { get; set; } = true;

    /// <summary>
    /// Specifies the default retry count for operations in case of failure.
    /// Can be adjusted based on future needs.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Specifies if caching should be performed using in-memory cache only.
    /// This can be expanded to support other mechanisms in the future.
    /// </summary>
    public bool UseInMemoryCache { get; set; } = true;
}

