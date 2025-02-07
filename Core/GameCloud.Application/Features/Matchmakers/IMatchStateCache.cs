namespace GameCloud.Application.Features.Matchmakers;


public interface IMatchStateCache
{
    Task<MatchState?> GetMatchStateAsync(Guid matchId);
    Task SetMatchStateAsync(Guid matchId, MatchState state, TimeSpan? expiry = null);
    Task RemoveMatchStateAsync(Guid matchId);
}
