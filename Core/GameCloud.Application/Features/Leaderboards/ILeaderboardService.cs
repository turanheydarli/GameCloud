using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Leaderboards.Requests;
using GameCloud.Application.Features.Leaderboards.Responses;
using GameCloud.Domain.Dynamics;

namespace GameCloud.Application.Features.Leaderboards;

public interface ILeaderboardService
{
    Task<LeaderboardResponse> CreateLeaderboardAsync(LeaderboardRequest request);
    Task<LeaderboardResponse> UpdateLeaderboardAsync(Guid leaderboardId, LeaderboardRequest request);
    Task DeleteLeaderboardAsync(Guid leaderboardId);

    Task<LeaderboardResponse?> GetLeaderboardAsync(Guid? leaderboardId = null, Guid? gameId = null,
        string? leaderboardName = null);

    Task<PageableListResponse<LeaderboardResponse>> GetPagedLeaderboardsAsync(DynamicRequest request);

    Task<LeaderboardRecordResponse> SubmitScoreAsync(Guid leaderboardId, LeaderboardRecordRequest request);
    Task<List<LeaderboardRecordResponse>> GetLeaderboardRecordsAsync(Guid leaderboardId, int? limit = 100, int? offset = 0);
    Task<LeaderboardRecordResponse> GetUserLeaderboardRecordAsync(Guid leaderboardId, Guid userId);
    
    Task ResetLeaderboardAsync(Guid leaderboardId);
    Task<LeaderboardResponse> GetLeaderboardByNameAsync(string name);
    Task<List<LeaderboardRecordResponse>> GetUserLeaderboardRecordsAsync(Guid userId, int? limit = 100);
}