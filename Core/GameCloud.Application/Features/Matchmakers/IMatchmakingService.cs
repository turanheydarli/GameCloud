using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using System.Text.Json;

namespace GameCloud.Application.Features.Matchmakers;

public interface IMatchmakingService
{
    // Queue Management
    Task<MatchmakingResponse> CreateQueueAsync(MatchQueueRequest queueRequest);
    Task<MatchmakingResponse?> GetQueueAsync(Guid? queueId = null, Guid? gameId = null, string? queueName = null);

    // Matchmaking
    Task<MatchTicketResponse> CreateTicketAsync(Guid gameId, Guid playerId, string queueName, JsonDocument? properties = null);
    Task<List<MatchResponse>> ProcessMatchmakingAsync(Guid? queueId = null);
    Task<MatchResponse> MarkPlayerReadyAsync(Guid matchId, Guid playerId);

    // Match Management
    Task<MatchResponse?> GetMatchAsync(Guid matchId);
    Task<JsonDocument> GetMatchStateAsync(Guid matchId);
    Task<MatchActionResponse> SubmitActionAsync(Guid matchId, Guid playerId, MatchActionRequest action);
    Task<List<MatchActionResponse>> GetMatchActionsAsync(Guid matchId, DateTime? since = null, int? limit = null);
}