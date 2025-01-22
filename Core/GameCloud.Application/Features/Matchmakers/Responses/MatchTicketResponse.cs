using System.Text.Json;

namespace GameCloud.Application.Features.Matchmakers.Responses;

public record MatchTicketResponse(
    Guid Id,
    Guid GameId,
    Guid PlayerId,
    string QueueName,
    string Status,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    Guid? MatchId,
    JsonDocument? CustomProperties
);