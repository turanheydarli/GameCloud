using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using System.Text.Json;

namespace GameCloud.Application.Features.Matchmakers;

public interface IMatchmakingService
{
    Task<MatchmakingResponse> CreateQueueAsync(MatchQueueRequest queueRequest);
    Task<MatchmakingResponse> UpdateQueueAsync(Guid queueId, MatchQueueRequest queueRequest);
    Task DeleteQueueAsync(Guid queueId);
    Task<MatchmakingResponse?> GetQueueAsync(Guid? queueId = null, Guid? gameId = null, string? queueName = null);
    Task<MatchTicketResponse> CreateTicketAsync(
        Guid gameId,
        Guid playerId,
        string queueName,
        JsonDocument? matchCriteria = null,
        JsonDocument? properties = null);

    Task<MatchResponse> FindMatchAsync(FindMatchRequest request, Guid playerId);
    Task CancelTicketAsync(Guid ticketId);
    Task<MatchTicketResponse?> GetTicketAsync(Guid ticketId);
    Task<List<MatchResponse>> ProcessMatchmakingAsync(Guid? queueId = null);

    Task AcceptMatchAsync(Guid ticketId);
    Task DeclineMatchAsync(Guid ticketId);
    Task<MatchResponse?> GetMatchAsync(Guid matchId);
    Task<MatchResponse?> CheckMatchStatusAsync(Guid ticketId);
    Task CancelMatchAsync(Guid matchId);

    Task<MatchResponse> UpdateMatchStateAsync(
        Guid matchId, 
        JsonDocument playerStates,
        JsonDocument matchState,
        Guid? nextPlayerId = null,
        DateTime? nextDeadline = null);

    Task<MatchActionResponse> SubmitActionAsync(
        Guid matchId,
        Guid playerId,
        MatchActionRequest action);

    Task<List<MatchActionResponse>> GetMatchActionsAsync(
        Guid matchId, 
        DateTime? since = null,
        int? limit = null);

    Task<JsonDocument> GetMatchStateAsync(Guid matchId);
    Task<JsonDocument> GetPlayerStateAsync(Guid matchId, Guid playerId);
    Task<List<MatchActionResponse>> GetTurnHistoryAsync(Guid matchId);

    Task<List<MatchResponse>> GetPlayerActiveMatchesAsync(Guid playerId);
    Task<List<MatchTicketResponse>> GetPlayerActiveTicketsAsync(Guid playerId);

    Task<bool> ValidatePlayerTurnAsync(Guid matchId, Guid playerId);
    Task ProcessTurnTimeoutsAsync();
}