using System.Text.Json.Serialization;
using GameCloud.Application.Features.Notifications.Requests;

namespace GameCloud.Application.Features.Functions.Responses;

public record FunctionResult(
    Guid Id,
    FunctionStatus Status,
    Dictionary<string, object>? Payload,
    Dictionary<string, List<EntityAttributeUpdate>>? EntityUpdates,
    List<NotificationRequest>? Notifications,
    FunctionError? Error
);

public record EntityAttributeUpdate(
    string Key,
    object? Value
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FunctionStatus
{
    Success,
    Failed,
    PartialSuccess,
    Pending
}