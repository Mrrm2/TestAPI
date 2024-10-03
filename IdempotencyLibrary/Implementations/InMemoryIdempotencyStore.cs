using System;
using System.Collections.Concurrent;
using TestAPI.IdempotencyLibrary.Interfaces;

namespace TestAPI.IdempotencyLibrary.Implementations;

public class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<(string ClientId, string IdempotencyKey), (object Response, DateTime Expiry)> _store = new();

    public Task<object> GetResponseAsync(string clientId, string idempotencyKey)
    {
        _store.TryGetValue((clientId, idempotencyKey), out var cachedEntry);

        // Check if the cached entry is expired
        if (cachedEntry.Expiry < DateTime.UtcNow)
        {
            _store.TryRemove((clientId, idempotencyKey), out _);
            return Task.FromResult<object>(null);
        }

        return Task.FromResult(cachedEntry.Response);
    }

    public Task SaveResponseAsync(string clientId, string idempotencyKey, object response, TimeSpan expiry)
    {
        var expiryTime = DateTime.UtcNow.Add(expiry);
        _store[(clientId, idempotencyKey)] = (response, expiryTime);
        return Task.CompletedTask;
    }
}
