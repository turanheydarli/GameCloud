using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using System.Text.Json;
using GameCloud.Application.Common.Paging;
using GameCloud.Application.Common.Responses;

namespace GameCloud.Application.Features.Matchmakers;

public interface IMatchmakingService
{
    Task<MatchmakingResponse> CreateQueueAsync(MatchQueueRequest queueRequest);
    Task<MatchmakingResponse?> GetQueueAsync(Guid? queueId = null, Guid? gameId = null, string? queueName = null);

    Task<MatchTicketResponse> CreateTicketAsync(Guid gameId, Guid playerId, string queueName, JsonDocument? properties = null);
    Task<List<MatchResponse>> ProcessMatchmakingAsync(Guid? queueId = null);
    Task<MatchResponse> MarkPlayerReadyAsync(Guid matchId, Guid playerId);
      Task<MatchResponse> UpdatePresenceAsync(Guid matchId, Guid playerId, string sessionId,        PresenceStatus status, JsonDocument meta);

    Task<MatchResponse?> GetMatchAsync(Guid matchId);
    Task<JsonDocument> GetMatchStateAsync(Guid matchId);
    Task<MatchActionResponse> SubmitActionAsync(Guid matchId, Guid playerId, MatchActionRequest action);
    Task<List<MatchActionResponse>> GetMatchActionsAsync(Guid matchId, DateTime? since = null, int? limit = null);
    Task<MatchTicketResponse?> GetTicket(Guid gameDi, Guid playerId, Guid ticketId);
    Task<MatchResponse> EndMatchAsync(Guid matchId, Guid playerId, JsonDocument? finalState = null);
    Task<MatchResponse> LeaveMatchAsync(Guid matchId, Guid playerId);

    Task<PageableListResponse<MatchmakingResponse>> GetQueuesAsync(Guid gameId, string? search, PageableRequest request);
    Task<MatchmakingResponse> GetQueueDetailsAsync(Guid queueId);
    Task<MatchmakingResponse> UpdateQueueAsync(Guid queueId, MatchQueueRequest request);
    Task DeleteQueueAsync(Guid queueId);
    Task<QueueToggleResponse> ToggleQueueAsync(Guid queueId, bool isEnabled);
    
    Task<MatchmakingStatsResponse> GetMatchmakingStatsAsync(Guid gameId, List<Guid>? queueIds, string timeRange);
    Task<QueueActivityResponse> GetQueueActivityAsync(Guid queueId, string timeRange);
    Task<PageableListResponse<MatchResponse>> GetQueueMatchesAsync(Guid queueId, string? status, PageableRequest request);
    Task<PageableListResponse<MatchTicketResponse>> GetQueueTicketsAsync(Guid queueId, PageableRequest request);
    Task<QueueDashboardResponse> GetQueueDashboardAsync(Guid queueId);
    
    Task<QueueFunctionsResponse> GetQueueFunctionsAsync(Guid queueId);
    Task UpdateQueueFunctionsAsync(Guid queueId, UpdateQueueFunctionsRequest request);
    
    Task<JsonDocument> GetQueueRulesAsync(Guid queueId);
    Task UpdateQueueRulesAsync(Guid queueId, JsonDocument rules);
    
    Task<QueueTestResponse> TestQueueAsync(Guid queueId, QueueTestRequest request);

}