using System.Text.Json;
using GameCloud.Domain.Entities.Matchmaking;


namespace GameCloud.Application.Features.Matchmakers.Requests;

public record MatchQueueRequest(
    Guid GameId,
    string Name,
    string Description,
    int MinPlayers,
    int MaxPlayers,
    TimeSpan TicketTTL,
    TimeSpan? TurnTimeout,
    TimeSpan? MatchTimeout,
    JsonDocument Criteria,
    JsonDocument Rules,
    bool IsEnabled = true
);

public record FindMatchRequest(
    string QueueName,
    JsonDocument? MatchCriteria = null,
    JsonDocument? Properties = null
);

public record UpdateMatchStateRequest(
    Guid MatchId,
    JsonDocument PlayerStates,
    JsonDocument MatchState
);

public record MatchActionRequest(
    string ActionType,
    JsonDocument ActionData
);