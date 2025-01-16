using GameCloud.Application.Features.Notifications.Requests;
using GameCloud.Domain.Enums;

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
    string Collection,
    string Key,
    string? Value
);