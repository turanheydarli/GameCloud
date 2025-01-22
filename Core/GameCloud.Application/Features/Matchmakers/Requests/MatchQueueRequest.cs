using System.Text.Json;
using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Application.Features.Matchmakers.Requests;

public record MatchQueueRequest(
    Guid GameId,
    string Name,
    int MinPlayers,
    int MaxPlayers,
    TimeSpan TicketTTL,
    MatchingCriteria Criteria);

public record OfflineMatchRequest(
    string? QueueName,
    JsonDocument? Criteria 
);

public record FindMatchRequest(
    string QueueName,
    JsonDocument? CustomProperties = null
);