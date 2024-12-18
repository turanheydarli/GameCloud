using System.Text.Json;

namespace GameCloud.Application.Features.Actions.Responses;

public record ActionResponse(
    Guid Id,
    Guid SessionId,
    Guid PlayerId,
    string ActionType,
    JsonDocument Parameters,
    JsonDocument Result,
    DateTime ExecutedAt
);