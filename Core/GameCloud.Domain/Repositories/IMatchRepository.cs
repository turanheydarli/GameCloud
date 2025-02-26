using System.Text.Json;
using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Domain.Repositories;

public class QueueActivityData
{
    public List<string> TimePoints { get; set; } = new();
    public List<int> MatchCounts { get; set; } = new();
    public List<int> PlayerCounts { get; set; } = new();
}

public class MatchmakingLogEntry
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string QueueName { get; set; }
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; }
    public Guid? MatchId { get; set; }
    public Guid? PlayerId { get; set; }
    public JsonDocument Details { get; set; }
    public string? ErrorMessage { get; set; }
}
public interface IMatchRepository
{
    Task<Match?> GetPlayerActiveMatchAsync(Guid playerId);
    Task<Match?> GetByIdAsync(Guid matchId);
    Task CreateAsync(Match match);
    Task UpdateAsync(Match match);
    Task<List<Match>> GetTimeoutMatchesAsync(DateTime utcNow);
    Task<List<Match>> GetPlayerActiveMatchesAsync(Guid playerId);
    Task<int> GetActiveMatchesCountAsync(Guid gameId, List<Guid>? queueIds = null);
    Task<QueueActivityData> GetQueueActivityAsync(Guid queueId, DateTime startDate, DateTime endDate);
    Task<IPaginate<Match>> GetQueueMatchesAsync(Guid queueId, string? status, int pageIndex, int pageSize);

    Task<IPaginate<MatchmakingLogEntry>> GetMatchmakingLogsAsync(
        Guid gameId,
        Guid? queueId,
        string? eventType,
        DateTime startDate,
        DateTime endDate,
        int pageIndex,
        int pageSize);
}