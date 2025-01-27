using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Domain.Repositories;


public interface IMatchActionRepository 
{
    Task<MatchAction> CreateAsync(MatchAction action);
    Task<MatchAction?> GetByIdAsync(Guid id);
    Task<MatchAction> UpdateAsync(MatchAction action);
    Task DeleteAsync(MatchAction action);
    
    Task<List<MatchAction>> GetMatchActionsAsync(Guid matchId, DateTime? since = null, int? limit = null);
    Task<List<MatchAction>> GetPlayerActionsAsync(Guid playerId, DateTime? since = null, int? limit = null);
    Task<MatchAction?> GetLatestActionAsync(Guid matchId);
    Task<int> GetActionCountAsync(Guid matchId);
}
