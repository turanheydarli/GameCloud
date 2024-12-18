using System.Text.Json;

namespace GameCloud.Application.Features.Actions.Requests;

public record ActionRequest(
    Guid SessionId,
    Guid PlayerId,
    string ActionType,
    JsonDocument Parameters
);