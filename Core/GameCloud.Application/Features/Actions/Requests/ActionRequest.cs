using System.Text.Json;

namespace GameCloud.Application.Features.Actions.Requests;

public record ActionRequest(
    Guid SessionId,
    string ActionType,
    JsonDocument Payload,
    string ClientVersion = "unknown",
    string ClientPlatform = "unknown"
);