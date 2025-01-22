using System.Text.Json;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Application.Features.Matchmakers.Responses;

public record MatchmakingResponse(
    Guid Id,
    Guid GameId,
    string Name,
    int MinPlayers,
    int MaxPlayers,
    TimeSpan TicketTTL,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MatchResponse(
    Guid Id,
    Guid GameId,
    string QueueName,
    string State,
    List<Guid> PlayerIds,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? UpdatedAt,
    JsonDocument? MatchData
);