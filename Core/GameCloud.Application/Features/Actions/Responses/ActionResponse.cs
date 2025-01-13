using System.Text.Json;

namespace GameCloud.Application.Features.Actions.Responses;

public record ActionResponse(
    Guid Id,
    Guid SessionId,
    Guid PlayerId,
    string ActionType,
    JsonDocument? Payload,
    JsonDocument? Result,
    DateTime ExecutedAt
);