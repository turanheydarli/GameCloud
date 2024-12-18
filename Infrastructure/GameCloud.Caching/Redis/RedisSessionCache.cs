using System.Text.Json;
using GameCloud.Application.Common.Interfaces;
using StackExchange.Redis;

namespace GameCloud.Caching.Redis;

public class RedisSessionCache(IConnectionMultiplexer redis) : ISessionCache
{
    public async Task UpdateSessionStateAsync(Guid sessionId, Dictionary<string, Dictionary<string, object>> changes)
    {
        var db = redis.GetDatabase();

        string key = $"session:{sessionId}:state";
        var existingJson = await db.StringGetAsync(key);
        var existingState = 
            existingJson.HasValue
            ? JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(existingJson)
            : new Dictionary<string, Dictionary<string, object>>();

        foreach (var category in changes)
        {
            if (existingState != null && !existingState.ContainsKey(category.Key))
                existingState[category.Key] = new Dictionary<string, object>();

            foreach (var kvp in category.Value)
            {
                if (existingState != null)
                    existingState[category.Key][kvp.Key] = kvp.Value;
            }
        }

        var newJson = JsonSerializer.Serialize(existingState);
        await db.StringSetAsync(key, newJson);
    }

    public async Task<Dictionary<string, Dictionary<string, object>>> GetSessionStateAsync(Guid sessionId)
    {
        var db = redis.GetDatabase();
        string key = $"session:{sessionId}:state";
        var json = await db.StringGetAsync(key);
        if (!json.HasValue) return new Dictionary<string, Dictionary<string, object>>();
        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
    }
}