using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using GameCloud.Domain.Entities.Matchmaking;
using System.Text.Json;

namespace GameCloud.Application.Features.Matchmakers;

public interface IMatchmakingService
{
    Task<MatchmakingResponse> CreateQueueAsync(MatchQueueRequest queueRequest);
    Task<MatchmakingResponse> UpdateQueueAsync(Guid queueId, MatchQueueRequest queueRequest);
    Task DeleteQueueAsync(Guid queueId);
    Task<MatchmakingResponse?> GetQueueAsync(Guid? queueId = null, Guid? gameId = null, string? queueName = null);
    Task<MatchResponse> CreateOfflineMatchAsync(OfflineMatchRequest request, Guid playerId);
    Task<MatchResponse> FindOrCreateMatchAsync(FindMatchRequest request, Guid playerId);

    Task<MatchTicketResponse> EnqueuePlayerAsync(
        Guid gameId,
        Guid playerId,
        string queueName,
        JsonDocument? customProperties = null);

    Task CancelTicketAsync(Guid ticketId);
    Task<MatchTicketResponse?> GetTicketAsync(Guid ticketId);


    Task AcceptMatchAsync(Guid ticketId);
    Task DeclineMatchAsync(Guid ticketId);

    Task<MatchResponse?> GetMatchAsync(Guid matchId);
    Task<MatchResponse> UpdateMatchStateAsync(Guid matchId, MatchState newState);
    Task CancelMatchAsync(Guid matchId);

    Task<List<MatchResponse>> ProcessMatchmakingAsync(Guid? queueId = null);

    Task<MatchResponse?> CheckMatchStatusAsync(Guid ticketId);
}