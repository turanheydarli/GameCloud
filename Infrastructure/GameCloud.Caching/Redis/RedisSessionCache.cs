using System.Text.Json;
using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Features.Sessions.Models;
using StackExchange.Redis;

namespace GameCloud.Caching.Redis;

public class RedisSessionCache : ISessionCache
{
    private readonly IConnectionMultiplexer redis;
    private const string SessionKeyPrefix = "auth:session:";
    
    public async Task<SessionInfo> GetSessionAsync(string refreshToken)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync($"{SessionKeyPrefix}{refreshToken}");
        
        return value.HasValue 
            ? JsonSerializer.Deserialize<SessionInfo>(value) 
            : null;
    }

    public async Task SetSessionAsync(string refreshToken, SessionInfo session)
    {
        var db = redis.GetDatabase();
        var value = JsonSerializer.Serialize(session);
        
        await db.StringSetAsync(
            $"{SessionKeyPrefix}{refreshToken}",
            value,
            expiry: TimeSpan.FromDays(30)  // Match the refresh token expiration
        );
    }

    public async Task RemoveSessionAsync(string refreshToken)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync($"{SessionKeyPrefix}{refreshToken}");
    }

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