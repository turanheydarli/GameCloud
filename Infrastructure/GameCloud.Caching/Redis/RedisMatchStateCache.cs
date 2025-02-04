using System.Text.Json;
using GameCloud.Application.Features.Matchmakers;
using StackExchange.Redis;

namespace GameCloud.Caching.Redis;

public class RedisMatchStateCache(IConnectionMultiplexer redis) : IMatchStateCache
{
    private const string KeyPrefix = "match:state:";
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromHours(24);

    public async Task<MatchState?> GetMatchStateAsync(Guid matchId)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync($"{KeyPrefix}{matchId}");
        return value.HasValue 
            ? JsonSerializer.Deserialize<MatchState>(value!) 
            : null;
    }

    public async Task SetMatchStateAsync(Guid matchId, MatchState state, TimeSpan? expiry = null)
    {
        var db = redis.GetDatabase();
        var json = JsonSerializer.Serialize(state);
        await db.StringSetAsync(
            $"{KeyPrefix}{matchId}",
            json,
            expiry ?? DefaultExpiry
        );
    }

    public async Task RemoveMatchStateAsync(Guid matchId)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync($"{KeyPrefix}{matchId}");
    }
}