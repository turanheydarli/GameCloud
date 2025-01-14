using GameCloud.Application.Features.Players.Responses;

namespace GameCloud.Application.Features.Players;

public interface IPlayerAttributeService
{
    Task<Dictionary<string, AttributeResponse>> GetCollectionAsync(string username, string collection);
    Task<AttributeResponse?> GetAsync(string username, string collection, string key);

    Task SetAsync(string username, string collection, string key, string value, string? expectedVersion = null,
        TimeSpan? ttl = null);

    Task SetBulkAsync(string username, string collection, Dictionary<string, string> attributes);
    Task DeleteAsync(string username, string collection, string key);
}