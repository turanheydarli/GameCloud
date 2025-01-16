using System.Text.Json;

namespace GameCloud.Application.Features.Actions.Requests;

public record ActionRequest(
    Guid SessionId,
    string ActionType,
    JsonElement Payload,
    string ClientVersion = "unknown",
    string ClientPlatform = "unknown"
);