using System;

namespace TestAPI.IdempotencyLibrary.Attributes
{
    /// <summary>
    /// Attribute to mark an action method for response capture and caching.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class ResponseCaptureAttribute : Attribute
    {
        /// <summary>
        /// Specifies the HTTP status codes for which the response should be captured.
        /// Defaults to 200 (OK).
        /// </summary>
        public int[] StatusCodesToCapture { get; set; } = { 200 };

        /// <summary>
        /// Determines whether to include response headers in the cached data.
        /// Defaults to false.
        /// </summary>
        public bool IncludeHeaders { get; set; } = false;

        /// <summary>
        /// Specifies the cache expiry duration in hours for the captured response.
        /// Defaults to 24 hours.
        /// </summary>
        public int CacheExpiryInHours { get; set; } = 24;

        /// <summary>
        /// Indicates if the captured response should include metadata (e.g., timestamp).
        /// Defaults to false.
        /// </summary>
        public bool IncludeMetadata { get; set; } = false;

        /// <summary>
        /// Determines whether caching should be performed using in-memory cache.
        /// Can be expanded to support distributed cache in the future.
        /// </summary>
        public bool UseInMemoryCache { get; set; } = true;

        /// <summary>
        /// Specifies if caching should occur asynchronously.
        /// Defaults to true.
        /// </summary>
        public bool CacheAsynchronously { get; set; } = true;

        /// <summary>
        /// Determines whether caching should only be done for successful operations.
        /// Defaults to true.
        /// </summary>
        public bool OnlyCacheSuccessfulResponses { get; set; } = true;
    }
}
