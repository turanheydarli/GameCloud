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
}