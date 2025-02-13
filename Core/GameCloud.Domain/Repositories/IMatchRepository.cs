using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Domain.Repositories;

public interface IMatchRepository
{
    Task<Match?> GetPlayerActiveMatchAsync(Guid playerId);
    Task<Match?> GetByIdAsync(Guid matchId);
    Task CreateAsync(Match match);
    Task UpdateAsync(Match match);
    Task<List<Match>> GetTimeoutMatchesAsync(DateTime utcNow);
    Task<List<Match>> GetPlayerActiveMatchesAsync(Guid playerId);
}