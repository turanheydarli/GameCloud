using GameCloud.Domain.Dynamics;
using GameCloud.Domain.Entities.Leaderboards;

namespace GameCloud.Domain.Repositories;

public interface ILeaderboardRepository
{
    Task<Leaderboard> CreateAsync(Leaderboard leaderboard);
    Task<Leaderboard?> GetByIdAsync(Guid id);
    Task<Leaderboard?> GetSingleAsync(Guid? leaderboardId, Guid? gameId, string? leaderboardName);
    Task<Leaderboard> UpdateAsync(Leaderboard leaderboard);
    Task DeleteAsync(Leaderboard leaderboard);
    Task<IPaginate<Leaderboard>> GetPagedDynamicLeaderboards(DynamicRequest request);

    // New methods
    Task<LeaderboardRecord> CreateRecordAsync(LeaderboardRecord record);
    Task<LeaderboardRecord> UpdateRecordAsync(LeaderboardRecord record);
    Task<List<LeaderboardRecord>> GetLeaderboardRecordsAsync(Guid leaderboardId, int? limit = 100, int? offset = 0);
    Task<LeaderboardRecord?> GetUserLeaderboardRecordAsync(Guid leaderboardId, Guid playerId);
    Task<List<LeaderboardRecord>> GetUserLeaderboardRecordsAsync(Guid userId, int? limit = 100);
    Task DeleteLeaderboardRecordsAsync(Guid leaderboardId);
}