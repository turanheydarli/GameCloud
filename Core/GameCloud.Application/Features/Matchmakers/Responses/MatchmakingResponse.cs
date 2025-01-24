using System.Text.Json;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Application.Features.Matchmakers.Responses;


public record MatchTicketResponse(
    Guid Id,
    Guid GameId,
    Guid PlayerId,
    string QueueName,
    string Status,
    JsonDocument? MatchCriteria,
    JsonDocument? Properties,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    Guid? MatchId
);

public record MatchmakingResponse(
    Guid Id,
    Guid GameId,
    string Name,
    string Description,
    bool IsEnabled,
    int MinPlayers,
    int MaxPlayers,
    TimeSpan TicketTTL,
    TimeSpan? TurnTimeout,
    TimeSpan? MatchTimeout,
    JsonDocument Criteria,
    JsonDocument Rules,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MatchResponse(
    Guid Id,
    Guid GameId,
    string QueueName,
    string State,
    List<Guid> PlayerIds,
    Guid? CurrentPlayerId,
    JsonDocument PlayerStates,
    JsonDocument MatchState,
    JsonDocument? TurnHistory,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? LastActionAt,
    DateTime? NextActionDeadline,
    DateTime? CompletedAt
);

public record MatchActionResponse(
    Guid Id,
    Guid MatchId,
    Guid PlayerId,
    string ActionType,
    JsonDocument ActionData,
    DateTime Timestamp
);