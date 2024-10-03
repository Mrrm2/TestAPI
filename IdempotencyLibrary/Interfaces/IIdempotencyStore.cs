
namespace TestAPI.IdempotencyLibrary.Interfaces;

public interface IIdempotencyStore
{
    Task<object> GetResponseAsync(string clientId, string idempotencyKey);
    Task SaveResponseAsync(string clientId, string idempotencyKey, object response, TimeSpan expiry);
}
